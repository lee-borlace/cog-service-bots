using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Watcher.Config
{
    public class CognitiveConfig
    {
        public string VisionSubscriptionKey { get; set; }

        public string VisionRegion { get; set; }

        public string FaceSubscriptionKey { get; set; }

        public string FaceRegion { get; set; }

        public string FacePersonGroupId { get; set; }

        public Dictionary<string, string> UserIdToNameMappings { get; set; }
    }
}
