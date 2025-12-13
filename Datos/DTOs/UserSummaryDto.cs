using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.DTOs
{
    public class UserSummaryDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }

        // ¿El usuario que está mirando la lista sigue a esta persona?
        public bool IsFollowing { get; set; }
    }
}
