﻿using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace ISpyBot.Controllers
{
    public class TokenController : ControllerBase
    {
        public async Task<ChatConfig> Index()
        {
            var secret = ConfigurationManager.AppSettings["Bot:DirectLineSecret"];
            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                $" https://directline.botframework.com/v3/directline/tokens/generate");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
            var userId = $"dl_{Guid.NewGuid()}";

            request.Content = new StringContent(
                JsonConvert.SerializeObject(
                    new { User = new { Id = userId } }),
                    Encoding.UTF8,
                    "application/json");
            var response = await client.SendAsync(request);
            string token = String.Empty;

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<DirectLineToken>(body).token;
            }

            var config = new ChatConfig()
            {
                Token = token,
                UserId = userId
            };

            return config;
        }


        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }



    public class ChatConfig
    {
        public string Token { get; set; }
        public string UserId { get; set; }
    }

    public class DirectLineToken
    {
        public string conversationId { get; set; }
        public string token { get; set; }
        public int expires_in { get; set; }
    }


}