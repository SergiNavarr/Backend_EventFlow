using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; } 
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int FollowersCount { get; set; } // Cuánta gente me sigue
        public int FollowingCount { get; set; } // A cuánta gente sigo
    }
}
