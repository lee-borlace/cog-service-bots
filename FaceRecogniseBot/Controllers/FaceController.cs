using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FaceRecogniseBot.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
//using Microsoft.ProjectOxford.Face;
//using Microsoft.ProjectOxford.Face.Contract;

namespace FaceRecogniseBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceController : ControllerBase
    {
        private FaceConfig _faceConfig;
        private FaceServiceClient _faceClient;

        public FaceController(IOptions<FaceConfig> options)
        {
            _faceConfig = options.Value;

            _faceClient = new FaceServiceClient(
                _faceConfig.ApiKey,
                $"https://{_faceConfig.Region}.api.cognitive.microsoft.com/face/v1.0/");
        }

        // Returns
        // Name of person if was able to identify someone
        // "unknown" if saw a face but couldn't identify it
        // empty string if didn't see a face
        [HttpPost("identifyMainFace")]
        public async Task<string> IdentifyMainFace()
        {
            try
            {
                var retVal = string.Empty;

                // Detect faces in the image.
                var faces = await _faceClient.DetectAsync(Request.Body);

                // Iterate over the faces seen. Look for the one with the biggest area in the image. Take this as the main face for ID purposes.
                if (faces != null && faces.Any())
                {
                    var largestFaceArea = 0;
                    Face mainFace = null;

                    foreach(var face in faces)
                    {
                        var faceArea = face.FaceRectangle.Width * face.FaceRectangle.Height;
                        
                        // Biggest face we've found so far.
                        if(faceArea > largestFaceArea)
                        {
                            largestFaceArea = faceArea;
                            mainFace = face;
                        }

                        // At this point we've found a face but we don't know who it is.
                        retVal = "unknown";
                    }

                    // Found a main face. Try to identify it.
                    if (mainFace != null)
                    {
                        var identifyResult = await _faceClient.IdentifyAsync(_faceConfig.PersonGroupId, new Guid[] { mainFace.FaceId });

                        if(identifyResult.Length > 0)
                        {
                            if(identifyResult[0].Candidates != null && identifyResult[0].Candidates.Any())
                            {
                                retVal = identifyResult[0].Candidates.First().PersonId.ToString();
                            }
                        }
                    }
                }

                return retVal;
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }

            return "test";
        }
    }
}