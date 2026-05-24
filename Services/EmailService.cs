using MimeKit;
using MailKit.Net.Smtp;
using Itihas360.Models;

namespace Itihas360.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) => _config = config;

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Itihas 360 Archives", _config["EmailSettings:SenderAddress"]));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            // Connects using settings defined securely inside your appsettings.json file
            await client.ConnectAsync(_config["EmailSettings:SmtpServer"],
                                      int.Parse(_config["EmailSettings:Port"] ?? "587"),
                                      MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_config["EmailSettings:Username"], _config["EmailSettings:Password"]);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}