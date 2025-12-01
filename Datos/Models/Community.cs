using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Models
{
    public class Community : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public string? CoverImageUrl { get; set; }

        // Creador/Dueño
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        // --- Relaciones ---
        public ICollection<UserCommunity> Members { get; set; } = new List<UserCommunity>();
        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
