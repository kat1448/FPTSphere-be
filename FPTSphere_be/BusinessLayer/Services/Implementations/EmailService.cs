using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, string? recipientName = null)
        {
            try
            {
                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // Cho phép HTML trong email
                };

                // Thêm người nhận
                if (!string.IsNullOrEmpty(recipientName))
                {
                    mailMessage.To.Add(new MailAddress(toEmail, recipientName));
                }
                else
                {
                    mailMessage.To.Add(toEmail);
                }

                await smtpClient.SendMailAsync(mailMessage);

                Console.WriteLine($"✅ Email sent successfully to: {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send email to {toEmail}: {ex.Message}");
                throw;
            }
        }

        public async Task SendEmailAsync(List<string> toEmails, string subject, string body)
        {
            foreach (var email in toEmails)
            {
                await SendEmailAsync(email, subject, body);
            }
        }
    }
}
