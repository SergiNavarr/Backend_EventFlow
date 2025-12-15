namespace Datos.DTOs
{
    // para devolver la url publica de la imagen subida a Cloudinary
    public class ImageUploadResponseDto
    {
        public string Url { get; set; }
        public string PublicId { get; set; }
    }
}