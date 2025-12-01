using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Models
{
    public class EventAttendee
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        // Status: "Going", "Maybe", "Interested", "Invited"
        public string RSVPStatus { get; set; } = "Going";

        public DateTime ResponseDate { get; set; } = DateTime.UtcNow;
    }
}
