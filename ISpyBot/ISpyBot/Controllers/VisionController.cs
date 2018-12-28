using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ISpyBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisionController : ControllerBase
    {
        [HttpPost("analyse")]
        public async Task<string> Analyse()
        {
            var reader = new StreamReader(Request.Body);
            var image = reader.ReadToEnd();

            return "";
        }
    }
}