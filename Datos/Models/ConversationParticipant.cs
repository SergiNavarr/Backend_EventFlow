using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Models
{
    public class ConversationParticipant
    {
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
