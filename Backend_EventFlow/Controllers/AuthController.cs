using Microsoft.AspNetCore.Mvc;
using Negocio.Services;
using Negocio.Interfaces;
using Datos.DTOs;

namespace Backend_EventFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            try
            {
                await _userService.Register(dto);
                return Created("api/auth/login", new { message = "Usuario registrado con éxito" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                var response = await _userService.Login(dto);
                // Devolvemos 200 OK con el Token y los datos
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Devolvemos 401 Unauthorized si la contraseña está mal
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
