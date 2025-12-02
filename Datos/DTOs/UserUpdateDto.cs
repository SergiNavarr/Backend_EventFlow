using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.DTOs
{
    public class UserUpdateDto
    {
        [MaxLength(500)]
        public string? Bio { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
