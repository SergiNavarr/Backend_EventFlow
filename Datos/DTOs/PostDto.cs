using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.DTOs
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Datos del Autor
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string? AuthorAvatar { get; set; }

        // Contexto 
        public int? CommunityId { get; set; }
        public string? CommunityName { get; set; }
        public int? EventId { get; set; }
        public string? EventTitle { get; set; }

        // Contadores 
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }

        // Estado del usuario actual
        public bool IsLikedByMe { get; set; }

        // true = Ya lo sigo (No mostrar botón o mostrar "Dejar de seguir")
        // false = No lo sigo (Mostrar botón "Seguir")
        public bool IsAuthorFollowedByMe { get; set; }

        //¿Este post es mío?
        public bool IsMine { get; set; }

        }
}
