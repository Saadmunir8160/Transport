using System.Net;
using System.Net.Mail;

namespace AjkTransportSystem.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var host = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
                var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
                var senderEmail = _configuration["Smtp:SenderEmail"] ?? "saadmunir8160@gmail.com";
                var senderName = _configuration["Smtp:SenderName"] ?? "AJK Transport System";
                var rawPassword = _configuration["Smtp:Password"] ?? "";
                var password = rawPassword.Replace(" ", "");

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Verification email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}
