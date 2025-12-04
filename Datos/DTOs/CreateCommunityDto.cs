using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.DTOs
{
    public class CreateCommunityDto
    {
        [Required(ErrorMessage = "El nombre de la comunidad es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
        public string Description { get; set; }

        // Opcional: URL de la imagen de portada (si el usuario no sube nada, quedará null)
        public string? CoverImageUrl { get; set; }
    }
}
