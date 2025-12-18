using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.DTOs
{
    public class UpdatePostDto
    {
        [Required(ErrorMessage = "El contenido no puede estar vacío")]
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
    }
}
