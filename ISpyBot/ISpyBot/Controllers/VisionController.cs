using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ISpyBot.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Options;

namespace ISpyBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisionController : ControllerBase
    {
        private VisionConfig _config;
        private ComputerVisionClient _visionClient;

        private static readonly List<VisualFeatureTypes> Features =
            new List<VisualFeatureTypes>()
        {
            VisualFeatureTypes.Tags
        };

        public VisionController(IOptions<VisionConfig> options)
        {
            _config = options.Value;

            _visionClient = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(_config.ApiKey),
                new System.Net.Http.DelegatingHandler[] { });

            _visionClient.Endpoint = $"https://{_config.Region}.api.cognitive.microsoft.com";
        }

        [HttpPost("analyse")]
        public async Task<ImageAnalysis> Analyse()
        {
            return await _visionClient.AnalyzeImageInStreamAsync(Request.Body, Features);
        }
    }
}