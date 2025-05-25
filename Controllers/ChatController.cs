using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WisVestAPI.Models;

namespace WisVestAPI.Controllers
{
    // public class ChatRequest
    // {
    //     public string Inputs { get; set; }
    // }

    [ApiController]
    [Route("api/chatbot")]
    public class ChatbotController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ChatbotController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Inputs))
                return BadRequest("Input question is empty.");

            var hfToken = _configuration["HuggingFaceToken"];
            if (string.IsNullOrWhiteSpace(hfToken))
                return StatusCode(500, "Hugging Face API token is not configured.");

            // Use a general-purpose model
            var modelName = "google/flan-t5-base";
            var apiUrl = $"https://api-inference.huggingface.co/models/{modelName}";

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hfToken);

            var payload = new { inputs = request.Inputs };
            var response = await client.PostAsJsonAsync(apiUrl, payload);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Hugging Face Error: " + errorDetails);
                return StatusCode((int)response.StatusCode, "Connot find the response fron the hugging face api");
            }

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Hugging Face Response: " + content);

            return Ok(content);
//             string reply = null;
// try
// {
//     // Hugging Face usually returns an array of objects with a 'generated_text' property
//     var json = System.Text.Json.JsonDocument.Parse(content);
//     if (json.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array &&
//         json.RootElement.GetArrayLength() > 0 &&
//         json.RootElement[0].TryGetProperty("generated_text", out var generatedText))
//     {
//         reply = generatedText.GetString();
//     }
// }
// catch
// {
//     // Ignore parsing errors for now
// }

// if (string.IsNullOrWhiteSpace(reply))
//     return Ok(new { reply = "Sorry, I could not find an answer." });

// return Ok(new { reply });
        }
    }
}
