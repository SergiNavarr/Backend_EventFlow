using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Models
{
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        public string? AvatarUrl { get; set; }

        // --- Relaciones ---

        // Comunidades que creó
        public ICollection<Community> OwnedCommunities { get; set; } = new List<Community>();
        // Comunidades a las que pertenece (Tabla intermedia)
        public ICollection<UserCommunity> Communities { get; set; } = new List<UserCommunity>();

        // Eventos que organiza
        public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
        // Eventos a los que asiste (Tabla intermedia)
        public ICollection<EventAttendee> EventAttendances { get; set; } = new List<EventAttendee>();

        // Contenido
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    }
}
