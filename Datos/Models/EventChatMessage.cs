using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Models
{
    public class EventChatMessage : BaseEntity
    {
        [Required]
        public string Content { get; set; }

        // Relación con el Evento
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        // Quién lo envió
        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;
    }
}
