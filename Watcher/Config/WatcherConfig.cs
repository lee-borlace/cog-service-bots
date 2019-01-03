using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Watcher.Config
{
    public class WatcherConfig
    {
        public CognitiveConfig CognitiveConfig { get; set; }

        public CosmosConfig CosmosConfig { get; set; }
    }
}
