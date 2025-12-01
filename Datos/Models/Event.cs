using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Models
{
    public class Event : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        [Required]
        public string Location { get; set; } // Puede ser dirección o URL

        public bool IsOnline { get; set; }
        public string? CoverImageUrl { get; set; }

        // Capacidad máxima (null = ilimitado)
        public int? MaxAttendees { get; set; }

        // Organizador
        public int OrganizerId { get; set; }
        public User Organizer { get; set; } = null!;

        // Comunidad opcional (si el evento es parte de una comunidad)
        public int? CommunityId { get; set; }
        public Community? Community { get; set; }

        // --- Relaciones ---
        public ICollection<EventAttendee> Attendees { get; set; } = new List<EventAttendee>();
        public ICollection<Post> Posts { get; set; } = new List<Post>(); // Posts dentro del evento
        public ICollection<EventChatMessage> ChatMessages { get; set; } = new List<EventChatMessage>();
    }
}
