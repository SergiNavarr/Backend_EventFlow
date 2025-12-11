using System.ComponentModel.DataAnnotations;

namespace Datos.DTOs
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Debes confirmar la nueva contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas nuevas no coinciden")]
        public string ConfirmNewPassword { get; set; }
    }
}