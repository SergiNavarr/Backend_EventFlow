using System.ComponentModel.DataAnnotations;

namespace Datos.DTOs
{
    // para recibir la imagen en Base64 y subirla a Cloudinary
    public class ImageUploadDto
    {
        [Required(ErrorMessage = "La imagen en Base64 es obligatoria")]
        public string ImageBase64 { get; set; }

        public string? Folder { get; set; }
    }
}