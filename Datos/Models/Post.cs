using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Models
{
    public class Post : BaseEntity
    {
        [Required]
        public string Content { get; set; }

        public string? ImageUrl { get; set; }

        // Autor
        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;

        // Contexto (Donde se publicó) - Opcionales
        public int? CommunityId { get; set; }
        public Community? Community { get; set; }

        public int? EventId { get; set; }
        public Event? Event { get; set; }

        // --- Relaciones ---
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    }
}
