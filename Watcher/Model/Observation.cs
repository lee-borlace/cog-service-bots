using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Watcher.Model
{
    /// <summary>
    /// Single observation from the watcher. Encapsulates vision and face API data.
    /// </summary>
    public class Observation
    {
        /// <summary>
        /// Results from image analysis.
        /// </summary>
        /// <value>
        /// The image analysis.
        /// </value>
        public ImageAnalysis ImageAnalysis { get; set; }

        /// <summary>
        /// Information about faces detected. The key is a face identified in the image, the value is an array of face identification results.
        /// </summary>
        /// <value>
        /// The faces.
        /// </value>
        public Dictionary<Face, List<IdentifyResult>> Faces { get; set; }

        public Exception Exception { get; set; }
    }
}
