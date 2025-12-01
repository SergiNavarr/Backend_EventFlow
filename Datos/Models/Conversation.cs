using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Models
{
    public class Conversation : BaseEntity
    {
        // "Private" o "Group" (por ahora Private)
        public string Type { get; set; } = "Private";

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public ICollection<DirectMessage> Messages { get; set; } = new List<DirectMessage>();
    }
}
