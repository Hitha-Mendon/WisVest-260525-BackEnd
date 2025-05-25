// using Microsoft.AspNetCore.Mvc;
// using YourNamespace.Services;
// using System.Threading.Tasks;

// namespace YourNamespace.Controllers
// {
//     [ApiController]
//     [Route("api/alerts")]
//     public class AlertController : ControllerBase
//     {
//         private readonly EmailService _emailService;

//         public AlertController(EmailService emailService)
//         {
//             _emailService = emailService;
//         }

//         [HttpPost("add")]
//         public async Task<IActionResult> AddAlert([FromBody] AlertRequest alert)
//         {
//             // Save alert logic (e.g., database) goes here

//             // Send email
//             var subject = $"📢 New Price Alert for {alert.Asset}";
//             var message = $"You’ve set an alert when {alert.Asset} crosses {alert.Threshold:C}.";

//             var success = await _emailService.SendEmailAsync(alert.Email, subject, message);
//             if (!success)
//                 return StatusCode(500, "Alert saved, but failed to send email.");

//             return Ok("Alert saved and email sent!");
//         }
//     }

//     public class AlertRequest
//     {
//         public string Email { get; set; }
//         public string Asset { get; set; }  // e.g., Gold, Bitcoin, Tesla
//         public decimal Threshold { get; set; }
//     }
// }
