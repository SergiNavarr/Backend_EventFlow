using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.DTOs
{
    public class EventDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string Location { get; set; }
        public bool IsOnline { get; set; }
        public string? CoverImageUrl { get; set; }
        public int? MaxAttendees { get; set; }

        // --- Datos Enriquecidos ---

        // Organizador
        public int OrganizerId { get; set; }
        public string OrganizerName { get; set; } // "Juan Perez"
        public string? OrganizerAvatar { get; set; }

        // Comunidad (Puede ser nulo)
        public int? CommunityId { get; set; }
        public string? CommunityName { get; set; } // "Club de Lectura"

        // Estadísticas
        public int AttendeesCount { get; set; } // Cuántos van

        // Estado del usuario actual (útil para el botón "Asistir")
        // Puede ser: "Going", "Maybe", "NotGoing" o null
        public string? MyRsvpStatus { get; set; }
    }
}
