using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Options;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Watcher.Config;
using Watcher.Data;
using Watcher.Model;

namespace Watcher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageAnalysisController : ControllerBase
    {
        private WatcherConfig _config;
        private FaceServiceClient _faceClient;
        private ComputerVisionClient _visionClient;
        private CosmosDataRepo _dataRepo;

        private static Observation _lastObservation;
        private static Observation _currentObservation;

        private static Dictionary<Guid, Person> _personDictionary = new Dictionary<Guid, Person>();

        private static readonly List<VisualFeatureTypes> VisionFeaturesToDetect =
           new List<VisualFeatureTypes>()
       {
            VisualFeatureTypes.Tags,
            VisualFeatureTypes.Categories,
            VisualFeatureTypes.Color,
            VisualFeatureTypes.Description,
            VisualFeatureTypes.ImageType,
            VisualFeatureTypes.Objects,
            VisualFeatureTypes.Tags,
            VisualFeatureTypes.Faces
       };

        //public ImageAnalysisController(
        //    IOptions<WatcherConfig> options,
        //    CosmosDataRepo dataRepo)
        //{
        //    _config = options.Value;

        //    _faceClient = new FaceServiceClient(
        //        _config.CognitiveConfig.FaceSubscriptionKey,
        //        $"https://{_config.CognitiveConfig.FaceRegion}.api.cognitive.microsoft.com/face/v1.0/");

        //    _visionClient = new ComputerVisionClient(
        //       new ApiKeyServiceClientCredentials(_config.CognitiveConfig.VisionSubscriptionKey),
        //       new System.Net.Http.DelegatingHandler[] { });

        //    _visionClient.Endpoint = $"https://{_config.CognitiveConfig.VisionRegion}.api.cognitive.microsoft.com";

        //    _dataRepo = dataRepo;
        //}

        public ImageAnalysisController(
      IOptions<WatcherConfig> options)
        {
            _config = options.Value;

            _faceClient = new FaceServiceClient(
                _config.CognitiveConfig.FaceSubscriptionKey,
                $"https://{_config.CognitiveConfig.FaceRegion}.api.cognitive.microsoft.com/face/v1.0/");

            _visionClient = new ComputerVisionClient(
               new ApiKeyServiceClientCredentials(_config.CognitiveConfig.VisionSubscriptionKey),
               new System.Net.Http.DelegatingHandler[] { });

            _visionClient.Endpoint = $"https://{_config.CognitiveConfig.VisionRegion}.api.cognitive.microsoft.com";
        }

        [HttpPost("analyse")]
        public async Task<Observation> Analyse()
        {
            var observation = new Observation();

            observation.Timestamp = DateTime.UtcNow;

            try
            {
                // Duplicate the stream.
                var faceStream = new MemoryStream();
                var visionStream = new MemoryStream();

                // Clone body stream so we can use several times.
                Request.EnableBuffering();

                Request.Body.CopyTo(faceStream);
                faceStream.Seek(0, SeekOrigin.Begin);

                Request.Body.Seek(0, SeekOrigin.Begin);
                Request.Body.CopyTo(visionStream);
                visionStream.Seek(0, SeekOrigin.Begin);
                Request.Body.Seek(0, SeekOrigin.Begin);

                // Kick off image analysis.
                var imageAnalysisTask = _visionClient.AnalyzeImageInStreamAsync(visionStream, VisionFeaturesToDetect);

                // Kick off and wait for face analysis.
                var faces = await _faceClient.DetectAsync(faceStream, returnFaceAttributes: new List<FaceAttributeType>() {
                    FaceAttributeType.Age,
                    FaceAttributeType.Emotion,
                    FaceAttributeType.FacialHair,
                    FaceAttributeType.Gender,
                    FaceAttributeType.Glasses,
                    FaceAttributeType.HeadPose,
                    FaceAttributeType.Smile
                });

                observation.Faces = new Dictionary<Guid, Face>();
                observation.FaceIdentifications = new Dictionary<Guid, List<IdentifyResult>>();


                if (faces != null && faces.Any())
                {
                    // Store info about each face.
                    foreach (var face in faces)
                    {
                        observation.Faces[face.FaceId] = face;
                    }

                    // All face IDs which came back.
                    var faceIds =
                        (from f in faces
                         select f.FaceId).ToArray();

                    // Try to identify faces. First call the API.
                    var faceIdentifyResults = await _faceClient.IdentifyAsync(_config.CognitiveConfig.FacePersonGroupId, faceIds);

                    var personIds = new List<Guid>();

                    // Go through results, map back to the faces we IDed initially.
                    if (faceIdentifyResults != null)
                    {
                        foreach (var faceIdentifyResult in faceIdentifyResults)
                        {
                            if (!observation.FaceIdentifications.ContainsKey(faceIdentifyResult.FaceId))
                            {
                                observation.FaceIdentifications[faceIdentifyResult.FaceId] = new List<IdentifyResult>();
                            }

                            var identifyResultForFace = observation.FaceIdentifications[faceIdentifyResult.FaceId];

                            identifyResultForFace.Add(faceIdentifyResult);

                            if (faceIdentifyResult.Candidates.Any())
                            {
                                personIds.Add(faceIdentifyResult.Candidates.First().PersonId);
                            }
                        }
                    }

                    // Remove any faces which weren't identified.
                    var faceIdsToRemove = new List<Guid>();
                    foreach (var faceId in observation.FaceIdentifications.Keys)
                    {
                        var identifications = observation.FaceIdentifications[faceId];

                        if (identifications == null || !identifications.Any() || !identifications.First().Candidates.Any())
                        {
                            faceIdsToRemove.Add(faceId);
                        }
                    }

                    foreach(var faceId in faceIdsToRemove)
                    {
                        observation.FaceIdentifications.Remove(faceId);
                    }

                    // Get info about any people we've encountered.
                    foreach(var personId in personIds)
                    {
                        if(!_personDictionary.ContainsKey(personId))
                        {
                            _personDictionary[personId] = await _faceClient.GetPersonAsync(_config.CognitiveConfig.FacePersonGroupId, personId);
                        }
                    }

                }

                // Make sure we've gotten the image analysis results.
                observation.ImageAnalysis = await imageAnalysisTask;

                _lastObservation = _currentObservation;
                _currentObservation = observation;

                observation.ObservationDelta = ObservationDelta.CalculateDelta(_lastObservation, _currentObservation, _personDictionary);

                // await _dataRepo.InsertObservation(observation);
            }
            catch (Exception ex)
            {
                observation.Exception = ex;
            }

            return observation;
        }
       
    }
}