using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISpyBot.Model
{
    public class VisionObject
    {
        [JsonProperty("object")]
        public string Obj { get; set; }
    }
}
