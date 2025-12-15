using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Datos.DTOs;
using Microsoft.Extensions.Configuration;
using Negocio.Interfaces;
using System;
using System.Threading.Tasks;

namespace Negocio.Services
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;

        //leer la configuración de Cloudinary desde appsettings.json
        public ImageService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<ImageUploadResponseDto> UploadImageAsync(string imageBase64, string folder = "general")
        {
            try
            {
                // configurar los parámetros de la imagen a subir
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription($"data:image/png;base64,{imageBase64}"),
                    Folder = folder,
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };
                // subir la imagen a Cloudinary
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception($"Error al subir imagen: {uploadResult.Error.Message}");
                }

                return new ImageUploadResponseDto
                {
                    Url = uploadResult.SecureUrl.ToString(),
                    PublicId = uploadResult.PublicId
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en upload: {ex.Message}");
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            try
            {

                var deleteParams = new DeletionParams(publicId);
                // eliminar la imagen de Cloudinary
                var result = await _cloudinary.DestroyAsync(deleteParams);
                return result.Result == "ok";
            }
            catch
            {
                return false;
            }
        }
    }
}