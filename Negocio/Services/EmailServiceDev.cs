using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Negocio.Interfaces;
using System;
using System.Threading.Tasks;

namespace Negocio.Services
{
    public class EmailServiceDev : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailServiceDev(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            //lee la configuración del email desde appsettings.json
            var settings = _config.GetSection("EmailSettings");

            // Crear el mensaje de email
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(settings["SenderName"], settings["SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            //crea el cuerpo del email
            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            // Conectar y enviar el email
            try
            {
                var host = settings["SmtpHost"];
                var port = int.Parse(settings["SmtpPort"]);
                var user = settings["SmtpUser"];
                var pass = settings["SmtpPass"];

                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(user, pass);
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                //solo se muestra error en consola para no romper la app ya que usamos error silencioso en forget-password
                Console.WriteLine($"[ERROR EMAIL] {ex.Message}");
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}