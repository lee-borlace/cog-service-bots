using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FaceRecogniseBot.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.ProjectOxford.Face;

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
                $"https://{_faceConfig.Region}.api.cognitive.microsoft.com");
        }

        [HttpPost("analyse")]
        public async Task<string> Analyse()
        {
            return string.Empty;
        }
    }
}