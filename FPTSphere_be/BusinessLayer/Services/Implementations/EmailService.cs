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
                // Remove spaces from password (Gmail App Password should not have spaces)
                var cleanPassword = _emailSettings.Password?.Replace(" ", "")?.Trim() ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(cleanPassword))
                {
                    throw new InvalidOperationException("Email password is not configured");
                }

                // Configure SMTP client with proper settings for Gmail
                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, cleanPassword),
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000 // 30 seconds timeout
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
            catch (SmtpException smtpEx)
            {
                var errorMessage = $"SMTP Error: {smtpEx.Message}";
                if (smtpEx.InnerException != null)
                {
                    errorMessage += $" Inner: {smtpEx.InnerException.Message}";
                }
                Console.WriteLine($"❌ Failed to send email to {toEmail}: {errorMessage}");
                Console.WriteLine($"   SMTP Server: {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");
                Console.WriteLine($"   Sender Email: {_emailSettings.SenderEmail}");
                throw new InvalidOperationException($"Failed to send email to {toEmail}. Please check SMTP configuration. Error: {smtpEx.Message}", smtpEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send email to {toEmail}: {ex.Message}");
                Console.WriteLine($"   SMTP Server: {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");
                Console.WriteLine($"   Sender Email: {_emailSettings.SenderEmail}");
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
