using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } // El JWT
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
