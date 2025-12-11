using Datos.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Interfaces
{
    public interface IUserService
    {
        // 1. REGISTRO
        // Recibe los datos, crea el usuario y guarda en BD.
        // No devuelve nada (Task), pero si falla lanzará una Excepción.
        Task Register(UserRegisterDto dto);

        // 2. LOGIN
        // Recibe email/pass y devuelve el objeto completo con el Token y datos básicos.
        Task<AuthResponseDto> Login(UserLoginDto dto);

        // 3. PERFIL
        // Recibe un ID y devuelve los datos públicos del usuario (incluyendo contadores de seguidores).
        Task<UserProfileDto> GetById(int userId);

        // 4. CAMBIO DE CONTRASEÑA
        // Recibe el ID del usuario y los datos necesarios para el cambio de contraseña.
        Task ChangePassword(int userId, ChangePasswordDto dto);

        // 5. RECUPERAR CONTRASEÑA
        // Recibe el email y devuelve un token para resetear la contraseña.
        Task<string> GenerateRecoveryToken(ForgotPasswordDto dto);

        // 6. RESETEAR CONTRASEÑA
        // Recibe el token y la nueva contraseña para actualizarla en BD.
        Task ResetPasswordWithToken(ResetPasswordDto dto);

        // 7. ACTUALIZAR PERFIL
        Task<UserProfileDto> UpdateUser(int userId, UserUpdateDto dto);

        // 8. BORRADO LOGICO DE USUARIO
        Task DeleteUser(int userId);
    }
}
