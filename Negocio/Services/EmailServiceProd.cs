using Microsoft.Extensions.Configuration;
using Negocio.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace Negocio.Services
{
    public class EmailServiceProd : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailServiceProd(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);
            
            var from = new EmailAddress(_config["SendGrid:FromEmail"], _config["SendGrid:FromName"]);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);
            
            try
            {
                await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EMAIL PROD] {ex.Message}");
            }
        }
    }
}

