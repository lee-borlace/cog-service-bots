using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watcher.Model
{
    /// <summary>
    /// Single observation from the watcher. Encapsulates vision and face API data.
    /// </summary>
    public class Observation
    {
        /// <summary>
        /// ID of camera that resulted in this observation.
        /// </summary>
        /// <value>
        /// The camera identifier.
        /// </value>
        public string CameraId { get; set; }

        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Results from image analysis.
        /// </summary>
        /// <value>
        /// The image analysis.
        /// </value>
        public ImageAnalysis ImageAnalysis { get; set; }

        /// <summary>
        /// Info about faces in image. Key is face ID and value is info about the face.
        /// </summary>
        /// <value>
        /// The faces.
        /// </value>
        public Dictionary<Guid, Face> Faces { get; set; }

        /// <summary>
        /// Info about identified faces. Key is face ID and value is identification candidates.
        /// </summary>
        /// <value>
        /// The face identifications.
        /// </value>
        public Dictionary<Guid, List<IdentifyResult>> FaceIdentifications { get; set; }

        public Exception Exception { get; set; }

        public ObservationDelta ObservationDelta { get; set; }

        /// <summary>
        /// Gets a list of names of identified people.
        /// </summary>
        /// <param name="userIdToNameMappings">The user identifier to name mappings.</param>
        /// <returns></returns>
        public List<string> GetIdentifiedPeopleList(Dictionary<string, string> userIdToNameMappings)
        {
            var retVal = new List<string>();

            if (FaceIdentifications != null)
            {
                var personIds = from faceIdentification
                           in FaceIdentifications.Values
                           select faceIdentification.First().Candidates[0].PersonId;

                foreach (var personId in personIds)
                {
                    if(userIdToNameMappings.ContainsKey(personId.ToString()))
                    {
                        retVal.Add(userIdToNameMappings[personId.ToString()]);
                    }
                }
            }

            return retVal;
        }

        private List<Guid> GetIdentifiedFaceIds()
        {
            var retVal = new List<Guid>();

            if (FaceIdentifications != null)
            {
                retVal = FaceIdentifications.Keys.ToList();
            }

            return retVal;
        }

        /// <summary>
        /// Gets a list of descriptions of unidentified people.
        /// </summary>
        /// <returns></returns>
        public List<string> GetUnidentifiedPeopleDescriptionList()
        {
            var retVal = new List<string>();

            if (FaceIdentifications != null && Faces != null)
            {
                var identifiedFaceIds = GetIdentifiedFaceIds();

                var unidentifiedFaceIds = Faces.Keys.Where(faceId => !identifiedFaceIds.Contains(faceId));

                foreach (var unidentifiedFaceId in unidentifiedFaceIds)
                {
                    retVal.Add(GetPersonDescription(Faces[unidentifiedFaceId]));
                }
            }

            return retVal;
        }

        public string GetPersonDescription(Face face)
        {
            return $"{face.FaceAttributes.Age} year old {face.FaceAttributes.Gender}.";
        }

        /// <summary>
        /// Gets a list of objects.
        /// </summary>
        /// <returns></returns>
        public List<string> GetObjectsList()
        {
            var retVal = new List<string>();

            if (ImageAnalysis != null && ImageAnalysis.Objects != null)
            {
                retVal = (from o in ImageAnalysis.Objects
                          select o.ObjectProperty).ToList();
            }

            return retVal;
        }



    }
}
