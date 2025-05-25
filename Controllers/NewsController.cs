// using Microsoft.AspNetCore.Mvc;
// using System.Net.Http;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Configuration;
// using System.Text.Json;

// namespace WisVestAPI.Controllers
// {
//     [ApiController]
//     [Route("api/news")]
//     public class NewsController : ControllerBase
//     {
//         private readonly IHttpClientFactory _httpClientFactory;
//         private readonly IConfiguration _configuration;

//         public NewsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
//         {
//             _httpClientFactory = httpClientFactory;
//             _configuration = configuration;
//         }

//         [HttpGet("financial")]
//         public async Task<IActionResult> GetFinancialNews()
//         {
//             var apiKey = _configuration["NewsApiKey"];
//             if (string.IsNullOrEmpty(apiKey))
//                 return StatusCode(500, "News API key is not configured.");

//             var url = $"https://newsapi.org/v2/top-headlines?country=us&category=business&apiKey={apiKey}";

//             var client = _httpClientFactory.CreateClient();
//             var response = await client.GetAsync(url);

//             if (!response.IsSuccessStatusCode)
//                 return StatusCode((int)response.StatusCode, "Failed to fetch news.");

//             var json = await response.Content.ReadAsStringAsync();
//             var newsData = JsonDocument.Parse(json);

//             return Ok(newsData.RootElement);
//         }
//     }
// }
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;
// using System.Net.Http;
// using System.Threading.Tasks;

// namespace YourNamespace.Controllers
// {
//     [ApiController]
//     [Route("api/news")]
//     public class NewsController : ControllerBase
//     {
//         private readonly IHttpClientFactory _httpClientFactory;
//         private readonly IConfiguration _configuration;

//         public NewsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
//         {
//             _httpClientFactory = httpClientFactory;
//             _configuration = configuration;
//         }

//         [HttpGet("financial")]
//         public async Task<IActionResult> GetFinancialNews()
//         {
//             var apiKey = _configuration["NewsApiKey"];

//             if (string.IsNullOrEmpty(apiKey))
//                 return BadRequest("News API key is missing from configuration.");

//             var requestUrl = "https://newsapi.org/v2/top-headlines?category=business&language=en";

//             var client = _httpClientFactory.CreateClient();

//             // ✅ Set required headers
//             client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
//             client.DefaultRequestHeaders.Add("User-Agent", "WisVestApp/1.0");

//             HttpResponseMessage response;

//             try
//             {
//                 response = await client.GetAsync(requestUrl);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, $"Error while calling News API: {ex.Message}");
//             }

//             if (!response.IsSuccessStatusCode)
//             {
//                 var error = await response.Content.ReadAsStringAsync();
//                 return StatusCode((int)response.StatusCode, $"News API error: {error}");
//             }

//             var content = await response.Content.ReadAsStringAsync();
//             return Ok(content);
//         }
//     }
// }

// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;
// using System.Net.Http;
// using System.Threading.Tasks;
// using System;

// namespace YourNamespace.Controllers
// {
//     [ApiController]
//     [Route("api/news")]
//     public class NewsController : ControllerBase
//     {
//         private readonly IHttpClientFactory _httpClientFactory;
//         private readonly IConfiguration _configuration;

//         public NewsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
//         {
//             _httpClientFactory = httpClientFactory;
//             _configuration = configuration;
//         }

//         [HttpGet("financial")]
//         public async Task<IActionResult> GetFinancialNews()
//         {
//             var apiKey = _configuration["NewsApiKey"];

//             if (string.IsNullOrEmpty(apiKey))
//                 return BadRequest("News API key is missing from configuration.");

//             // Include pageSize=5 in URL query params
//             var requestUrl = "https://newsapi.org/v2/top-headlines?category=business&language=en&pageSize=5";

//             var client = _httpClientFactory.CreateClient();

//             // Set required headers (apiKey and User-Agent)
//             client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
//             client.DefaultRequestHeaders.Add("User-Agent", "WisVestApp/1.0");

//             HttpResponseMessage response;

//             try
//             {
//                 response = await client.GetAsync(requestUrl);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, $"Error while calling News API: {ex.Message}");
//             }

//             if (!response.IsSuccessStatusCode)
//             {
//                 var error = await response.Content.ReadAsStringAsync();
//                 return StatusCode((int)response.StatusCode, $"News API error: {error}");
//             }

//             var content = await response.Content.ReadAsStringAsync();
//             return Ok(content);
//         }
//     }
// }
// [HttpGet("financial")]
// public async Task<IActionResult> GetFinancialNews()
// {
//     var apiKey = _configuration["NewsApiKey"];

//     if (string.IsNullOrEmpty(apiKey))
//         return BadRequest("News API key is missing from configuration.");

//     // Query parameter for finance and investment keywords
//     var requestUrl = "https://newsapi.org/v2/top-headlines?q=finance OR investment&language=en&pageSize=5";

//     var client = _httpClientFactory.CreateClient();

//     client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
//     client.DefaultRequestHeaders.Add("User-Agent", "WisVestApp/1.0");

//     HttpResponseMessage response;

//     try
//     {
//         response = await client.GetAsync(requestUrl);
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, $"Error while calling News API: {ex.Message}");
//     }

//     if (!response.IsSuccessStatusCode)
//     {
//         var error = await response.Content.ReadAsStringAsync();
//         return StatusCode((int)response.StatusCode, $"News API error: {error}");
//     }

//     var content = await response.Content.ReadAsStringAsync();
//     return Ok(content);
// }
//     }
// }
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;
// using System.Net.Http;
// using System.Threading.Tasks;

// namespace YourNamespace.Controllers
// {
//     [ApiController]
//     [Route("api/news")]
//     public class NewsController : ControllerBase
//     {
//         private readonly IHttpClientFactory _httpClientFactory;
//         private readonly IConfiguration _configuration;

//         public NewsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
//         {
//             _httpClientFactory = httpClientFactory;
//             _configuration = configuration;
//         }

//         [HttpGet("financial")]
//         public async Task<IActionResult> GetFinancialNews()
//         {
//             var apiKey = _configuration["NewsApiKey"];

//             if (string.IsNullOrEmpty(apiKey))
//                 return BadRequest("News API key is missing from configuration.");

//             var requestUrl = "https://newsapi.org/v2/top-headlines?category=business&language=en";

//             var client = _httpClientFactory.CreateClient();

//             // ✅ Set required headers
//             client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
//             client.DefaultRequestHeaders.Add("User-Agent", "WisVestApp/1.0");

//             HttpResponseMessage response;

//             try
//             {
//                 response = await client.GetAsync(requestUrl);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, $"Error while calling News API: {ex.Message}");
//             }

//             if (!response.IsSuccessStatusCode)
//             {
//                 var error = await response.Content.ReadAsStringAsync();
//                 return StatusCode((int)response.StatusCode, $"News API error: {error}");
//             }

//             var content = await response.Content.ReadAsStringAsync();
//             return Ok(content);
//         }
//     }
// }

// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;
// using System.Net.Http;
// using System.Threading.Tasks;

// namespace YourNamespace.Controllers
// {
//     [ApiController]
//     [Route("api/news")]
//     public class NewsController : ControllerBase
//     {
//         private readonly IHttpClientFactory _httpClientFactory;
//         private readonly IConfiguration _configuration;

//         public NewsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
//         {
//             _httpClientFactory = httpClientFactory;
//             _configuration = configuration;
//         }

//         [HttpGet("financial")]
//         public async Task<IActionResult> GetFinancialNews()
//         {
//             var apiKey = _configuration["FinnhubApiKey"];  // Changed to Finnhub key

//             if (string.IsNullOrEmpty(apiKey))
//                 return BadRequest("Finnhub API key is missing from configuration.");

//             // Finnhub news endpoint - general category (you can filter in frontend/backend)
//             var requestUrl = $"https://finnhub.io/api/v1/news?category=general&token={apiKey}";

//             var client = _httpClientFactory.CreateClient();

//             HttpResponseMessage response;

//             try
//             {
//                 response = await client.GetAsync(requestUrl);
//             }
//             catch (System.Exception ex)
//             {
//                 return StatusCode(500, $"Error while calling Finnhub API: {ex.Message}");
//             }

//             if (!response.IsSuccessStatusCode)
//             {
//                 var error = await response.Content.ReadAsStringAsync();
//                 return StatusCode((int)response.StatusCode, $"Finnhub API error: {error}");
//             }

//             var content = await response.Content.ReadAsStringAsync();
//             return Ok(content);
//         }
//     }
// }


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/news")]
    public class NewsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public NewsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet("financial")]
        public async Task<IActionResult> GetFinancialNews()
        {
            var apiKey = _configuration["FinnhubApiKey"];

            if (string.IsNullOrEmpty(apiKey))
                return BadRequest("Finnhub API key is missing from configuration.");

            var requestUrl = $"https://finnhub.io/api/v1/news?category=general&token={apiKey}";

            var client = _httpClientFactory.CreateClient();

            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync(requestUrl);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error while calling Finnhub API: {ex.Message}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Finnhub API error: {error}");
            }

            var content = await response.Content.ReadAsStringAsync();

            var articles = JsonSerializer.Deserialize<Article[]>(content);

            var filteredArticles = articles
                .Where(a => a.headline != null && (
                    a.headline.ToLower().Contains("stock") ||
                    a.headline.ToLower().Contains("commodity") ||
                    a.headline.ToLower().Contains("oil") ||
                    a.headline.ToLower().Contains("gold") ||
                    a.headline.ToLower().Contains("silver") ||
                    a.headline.ToLower().Contains("crude")))
                .ToArray();

            return Ok(filteredArticles);
        }

        [HttpGet("company/{symbol}")]
        public async Task<IActionResult> GetCompanyNews(string symbol, string from = null, string to = null)
        {
            var apiKey = _configuration["FinnhubApiKey"];

            if (string.IsNullOrEmpty(apiKey))
                return BadRequest("Finnhub API key is missing from configuration.");

            from ??= System.DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            to ??= System.DateTime.UtcNow.ToString("yyyy-MM-dd");

            var requestUrl = $"https://finnhub.io/api/v1/company-news?symbol={symbol}&from={from}&to={to}&token={apiKey}";

            var client = _httpClientFactory.CreateClient();

            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync(requestUrl);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error while calling Finnhub API: {ex.Message}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Finnhub API error: {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }

        private class Article
        {
            public string headline { get; set; }
            public string summary { get; set; }
            public string url { get; set; }
            public long datetime { get; set; }
            public string source { get; set; }
        }
    }
}

