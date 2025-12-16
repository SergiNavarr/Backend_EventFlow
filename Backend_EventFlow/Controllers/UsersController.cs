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
                int? currentUserId = null;
                // 1. "Abrimos" el token para sacar el ID del usuario
                // ClaimTypes.NameIdentifier es donde guardamos el ID en el UserService
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                int userId = int.Parse(userIdClaim.Value);

                // 2. Buscamos los datos
                var profile = await _userService.GetById(userId, currentUserId);

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
                int? currentUserId = null;
                var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

                //  Si encontramos el claim y es un número válido, lo asignamos
                if (claimId != null && int.TryParse(claimId.Value, out int parsedId))
                {
                    currentUserId = parsedId;
                }
                // Si el usuario estaba logueado, 'currentUserId' tendrá un número
                // Si no, seguirá siendo 'null'
                var profile = await _userService.GetById(id, currentUserId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }
        }

        // PUT: api/users/change-password
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] Datos.DTOs.ChangePasswordDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();

                int userId = int.Parse(userIdClaim.Value);

                await _userService.ChangePassword(userId, dto);

                return Ok(new { message = "Contraseña actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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

        // POST: api/users/5/follow
        [HttpPost("{id}/follow")]
        public async Task<IActionResult> Follow(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await _userService.FollowUserAsync(id, currentUserId);
                return Ok(new { message = "Ahora sigues a este usuario." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/users/5/follow (Unfollow)
        [HttpDelete("{id}/follow")]
        public async Task<IActionResult> Unfollow(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await _userService.UnfollowUserAsync(id, currentUserId);
                return Ok(new { message = "Has dejado de seguir a este usuario." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/users/5/followers
        [HttpGet("{id}/followers")]
        public async Task<IActionResult> GetFollowers(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var list = await _userService.GetFollowersAsync(id, currentUserId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/users/5/following
        [HttpGet("{id}/following")]
        public async Task<IActionResult> GetFollowing(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var list = await _userService.GetFollowingAsync(id, currentUserId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/users/search?query=texto
        // GET: api/users/search?query=juan
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            try
            {
                var results = await _userService.SearchUsers(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
