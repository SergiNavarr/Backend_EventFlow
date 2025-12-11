using System.ComponentModel.DataAnnotations;

namespace Datos.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }
    }
}