using Datos.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Interfaces
{
    public interface IChatService
    {
        // Obtener historial del chat de un evento
        Task<List<EventMessageDto>> GetEventMessages(int eventId, int currentUserId);

        // Enviar mensaje
        Task<EventMessageDto> SendEventMessage(int eventId, int userId, string content);
    }
}