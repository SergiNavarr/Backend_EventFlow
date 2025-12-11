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

        // Actualiza perfil (Bio, Avatar)
        Task<UserProfileDto> UpdateUser(int userId, UserUpdateDto dto);

        // Borrado lógico de la cuenta
        Task DeleteUser(int userId);
    }
}
