// // Services/AlertMonitorService.cs
// public class AlertMonitorService : BackgroundService
// {
//     private readonly IHttpClientFactory _clientFactory;
//     private readonly AlertStorageService _storageService;
//     private readonly EmailService _emailService;
//     private readonly IConfiguration _config;

//     public AlertMonitorService(IHttpClientFactory clientFactory, AlertStorageService storage, EmailService emailService, IConfiguration config)
//     {
//         _clientFactory = clientFactory;
//         _storageService = storage;
//         _emailService = emailService;
//         _config = config;
//     }

//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             var alerts = _storageService.GetAlerts();

//             foreach (var alert in alerts.Where(a => !a.IsTriggered))
//             {
//                 var price = await GetCurrentPrice(alert.AssetSymbol, alert.AssetType);

//                 if (IsConditionMet(price, alert.Condition, alert.Threshold))
//                 {
//                     await _emailService.SendEmailAsync(
//                         alert.UserEmail,
//                         $"Price Alert Triggered for {alert.AssetSymbol}",
//                         $"The price of {alert.AssetSymbol} is now {price} and matched your condition: {alert.Condition} {alert.Threshold}");

//                     alert.IsTriggered = true;
//                 }
//             }

//             _storageService.SaveAlerts(alerts);

//             await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Check every 5 mins
//         }
//     }

//     private async Task<decimal> GetCurrentPrice(string symbol, string type)
//     {
//         var apiKey = _config["FinnhubApiKey"];
//         var client = _clientFactory.CreateClient();

//         string url = type == "stock"
//             ? $"https://finnhub.io/api/v1/quote?symbol={symbol}&token={apiKey}"
//             : $"https://finnhub.io/api/v1/commodity/price?symbol={symbol}&token={apiKey}";

//         var response = await client.GetFromJsonAsync<JsonElement>(url);
//         return response.TryGetProperty("c", out var priceProp) ? priceProp.GetDecimal() : 0;
//     }

//     private bool IsConditionMet(decimal price, string condition, decimal threshold)
//     {
//         return condition switch
//         {
//             ">" => price > threshold,
//             "<" => price < threshold,
//             "=" => price == threshold,
//             _ => false
//         };
//     }
// }
