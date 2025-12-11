using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.DTOs
{
    public class EventMessageDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // Datos del remitente (para mostrar foto y nombre en el chat)
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string? SenderAvatar { get; set; }

        // Booleano útil para el frontend 
        public bool IsMine { get; set; }
    }
}
