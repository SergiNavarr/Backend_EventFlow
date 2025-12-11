using System.ComponentModel.DataAnnotations;

namespace Datos.DTOs
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "El token es obligatorio")]
        public string Token { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Debes confirmar la contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmNewPassword { get; set; }
    }
}