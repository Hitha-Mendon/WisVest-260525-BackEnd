// using Microsoft.Extensions.Configuration;
// using SendGrid;
// using SendGrid.Helpers.Mail;
// using System.Threading.Tasks;

// namespace YourNamespace.Services
// {
//     public class EmailService
//     {
//         private readonly IConfiguration _configuration;

//         public EmailService(IConfiguration configuration)
//         {
//             _configuration = configuration;
//         }

//         public async Task<bool> SendEmailAsync(string toEmail, string subject, string content)
//         {
//             var apiKey = _configuration["SendGrid:ApiKey"];
//             var fromEmail = _configuration["SendGrid:FromEmail"];
//             var fromName = _configuration["SendGrid:FromName"];

//             var client = new SendGridClient(apiKey);
//             var from = new EmailAddress(fromEmail, fromName);
//             var to = new EmailAddress(toEmail);
//             var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);

//             var response = await client.SendEmailAsync(msg);
//             return response.IsSuccessStatusCode;
//         }
//     }
// }
