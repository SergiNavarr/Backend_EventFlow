using System.ComponentModel.DataAnnotations;

namespace Datos.DTOs
{
    public class ImageUploadDto
    {
        [Required(ErrorMessage = "La imagen en Base64 es obligatoria")]
        public string ImageBase64 { get; set; }

        public string? Folder { get; set; }
    }
}