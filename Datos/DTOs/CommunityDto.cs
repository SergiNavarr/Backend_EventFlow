using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.DTOs
{
    public class CommunityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? CoverImageUrl { get; set; }

        // --- Datos calculados o de relaciones ---

        public int MemberCount { get; set; } // Calcularemos esto en el servicio

        // Información básica del Creador/Dueño
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } // Para mostrar "Creado por Juan"

        public DateTime CreatedAt { get; set; }

        // Extra útil para el futuro: 
        // ¿El usuario que está viendo esto ya es miembro?
        public bool IsMember { get; set; }
    }
}
