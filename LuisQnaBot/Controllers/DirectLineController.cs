using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Intelequia.Core.TurismoTenerife.Bot.Controllers
{
    [Route("api/directline")]
    [ApiController]
    public class DirectLineController
    {
        private readonly IConfiguration _config;

        public DirectLineController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("generateToken")]
        public async Task<DirectLineToken> GenerateToken()
        {
            var secret = _config.GetValue<string>("WebChatKey1");

            using (var client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(
              HttpMethod.Post,
              $" https://directline.botframework.com/v3/directline/tokens/generate");

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
                var response = await client.SendAsync(request);
                DirectLineToken directLineToken = new DirectLineToken();

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    directLineToken = JsonConvert.DeserializeObject<DirectLineToken>(body);
                }
                return directLineToken;
            }
        }

        [HttpGet("reconnect/{conversationId}")]
        public async Task<DirectLineToken> Reconnect(string conversationId, int watermark)
        {
            var secret = _config.GetValue<string>("WebChatKey1");

            using (var client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(
              HttpMethod.Get,
              $"https://directline.botframework.com/v3/directline/conversations/{conversationId}?watermark={watermark}");

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
                var response = await client.SendAsync(request);
                DirectLineToken directLineToken = new DirectLineToken();
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    directLineToken = JsonConvert.DeserializeObject<DirectLineToken>(body);
                }
                return directLineToken;
            }
        }

    }

    public class DirectLineToken
    {
        public string conversationId { get; set; }
        public string token { get; set; }
        public int expires_in { get; set; }
    }

    public class ChatConfig
    {
        public string Token { get; set; }
    }

    public class SpeechToken
    {
        public string region { get; set; }
        public string authorizationToken { get; set; }
    }
}
