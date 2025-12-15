using Datos.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Interfaces;
using System;
using System.Threading.Tasks;

namespace Backend_EventFlow.Controllers
{
    [Route("api/[controller]")] // Ruta: api/images
    [ApiController]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImagesController(IImageService imageService)
        {
            _imageService = imageService;
        }

        // SUBIR IMAGEN
        // POST: api/images/upload

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromBody] ImageUploadDto dto)
        {
            try
            {
                var result = await _imageService.UploadImageAsync(dto.ImageBase64, dto.Folder ?? "general");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ELIMINAR IMAGEN
        // DELETE: api/images?publicId=...
        [HttpDelete]
        public async Task<IActionResult> DeleteImage([FromQuery] string publicId)
        {
            try
            {
                var success = await _imageService.DeleteImageAsync(publicId);
                if (success)
                    return Ok(new { message = "Imagen eliminada" });
                else
                    return BadRequest(new { message = "No se pudo eliminar la imagen" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}