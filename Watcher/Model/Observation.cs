﻿using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
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
    }
}
