using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Models
{
    public class UserFollow
    {
        // Quién realiza la acción de seguir
        public int FollowerId { get; set; }
        public User Follower { get; set; } = null!;

        // A quién están siguiendo
        public int FollowedId { get; set; }
        public User Followed { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
