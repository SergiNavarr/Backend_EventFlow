using Datos.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Interfaces;
using System.Security.Claims;

namespace Backend_EventFlow.Controllers
{
    [Route("api/[controller]")] // La ruta será: api/users
    [ApiController]
    [Authorize] // <--- ¡CANDADO PUESTO! Solo entra gente con Token
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users/profile
        // Obtiene el perfil del usuario que está logueado actualmente
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                // 1. "Abrimos" el token para sacar el ID del usuario
                // ClaimTypes.NameIdentifier es donde guardamos el ID en el UserService
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                int userId = int.Parse(userIdClaim.Value);

                // 2. Buscamos los datos
                var profile = await _userService.GetById(userId);

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/users/5
        // Obtiene el perfil de CUALQUIER usuario por su ID (para ver perfiles ajenos)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserProfile(int id)
        {
            try
            {
                var profile = await _userService.GetById(id);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }
        }

        // PUT: api/users/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var updatedProfile = await _userService.UpdateUser(userId, dto);
                return Ok(updatedProfile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/users/profile
        [HttpDelete("profile")]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await _userService.DeleteUser(userId);
                return Ok(new { message = "Tu cuenta ha sido eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
