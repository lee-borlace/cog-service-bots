using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ISpyBot.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ISpyBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeechTokenController : ControllerBase
    {
        private SpeechConfig _config;

        public SpeechTokenController(IOptions<SpeechConfig> options)
        {
            _config = options.Value;
        }

        [HttpPost]
        public async Task<SpeechAuthResult> Post()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _config.ApiKey);

            var tokenUrl = $"https://{_config.Region}.api.cognitive.microsoft.com/sts/v1.0/issueToken";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);

            var retVal = new SpeechAuthResult();

            var response = await client.SendAsync(request);

            if(response.IsSuccessStatusCode)
            {
                retVal.Token = await response.Content.ReadAsStringAsync();
                retVal.Region = _config.Region;
            }

            return retVal;
        }
    }



    public class SpeechAuthResult
    {
        public string Token { get; set; }
        public string Region { get; set; }

    }

    

}