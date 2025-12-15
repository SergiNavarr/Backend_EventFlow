using Datos.DTOs;
using System.Threading.Tasks;

namespace Negocio.Interfaces
{
    public interface IImageService
    {
        // subir imagen a Cloudinary
        Task<ImageUploadResponseDto> UploadImageAsync(string imageBase64, string folder = "general");

        // eliminar imagen de Cloudinary
        Task<bool> DeleteImageAsync(string publicId);
    }
}