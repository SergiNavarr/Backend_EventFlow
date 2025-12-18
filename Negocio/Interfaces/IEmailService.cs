using System.Threading.Tasks;

namespace Negocio.Interfaces
{
    public interface IEmailService
    {
        // Enviar email genérico
        //recibe destinatario, asunto, cuerpo y si es HTML o texto plano
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    }
}