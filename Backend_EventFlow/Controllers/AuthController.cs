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
        private readonly IEmailService _emailService;

        public AuthController(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
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

                return Ok(response);
            }
            catch (Exception ex)
            {

                return Unauthorized(new { message = ex.Message });
            }
        }

        // POST: api/auth/forgot-password 
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            try
            {

                string token = await _userService.GenerateRecoveryToken(dto);

                var resetLink = $"http://localhost:3000/reset-password?token={token}";
                var htmlBody = $@"
                    <h2>Recuperación de Contraseña</h2>
                    <p>Hacé clic en el siguiente enlace para restablecer tu contraseña:</p>
                    <a href='{resetLink}'>Restablecer Contraseña</a>
                    <p>Si no solicitaste este cambio, ignorá este mensaje.</p>
                ";

                await _emailService.SendEmailAsync(dto.Email, "Recuperación de Contraseña", htmlBody);

                return Ok(new { message = "Si el correo existe, se enviará un enlace de recuperación." });
            }
            catch (Exception)
            {
                return Ok(new { message = "Si el correo existe, se enviará un enlace de recuperaciónnnn." });
            }
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _userService.ResetPasswordWithToken(dto);
                
                return Ok(new { message = "Contraseña restablecida con éxito. Ya puedes iniciar sesión." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
