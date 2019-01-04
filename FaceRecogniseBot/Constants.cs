using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FaceRecogniseBot
{
    public class Constants
    {
        public class BotEvents
        {
            public const string FacesAnalysed = "facesAnalysed";
            public const string FaceError = "faceError";
            public const string ReadyForCamera = "readyForCamera";
            public const string NewEmotion = "newEmotion";
        }
    }
}
