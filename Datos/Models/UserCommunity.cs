using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Models
{
    public class UserCommunity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int CommunityId { get; set; }
        public Community Community { get; set; } = null!;

        // Rol en la comunidad: "Member", "Moderator", "Admin"
        public string Role { get; set; } = "Member";

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
