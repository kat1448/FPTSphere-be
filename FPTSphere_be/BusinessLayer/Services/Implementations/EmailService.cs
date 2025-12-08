using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body, string? displayName = null)
        {
            // TODO: Sau này bạn thay phần này bằng code gửi mail thật (SMTP, SendGrid,...)
            _logger.LogInformation(
                "FAKE EMAIL -> To: {Email}, Subject: {Subject}\nBody:\n{Body}",
                toEmail, subject, body);

            return Task.CompletedTask;
        }
    }
}
