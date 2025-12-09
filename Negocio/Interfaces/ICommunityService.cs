using Datos.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Interfaces
{
    public interface ICommunityService
    {
        // 1. CREAR
        // Recibe el DTO con los datos y el ID del usuario que la crea (el dueño).
        // Devuelve la comunidad creada ya convertida a DTO (con su ID generado).
        Task<CommunityDto> CreateCommunityAsync(CreateCommunityDto dto, int userId);

        // 2. LISTAR TODAS (Explorar)
        // Devuelve la lista de todas las comunidades activas.
        Task<List<CommunityDto>> GetAllCommunitiesAsync();

        // 3. VER DETALLE
        // Busca una comunidad por ID.
        Task<CommunityDto> GetByIdAsync(int id);

        // 4. MIS COMUNIDADES
        // Devuelve las comunidades creadas por un usuario específico (filtro por dueño).
        Task<List<CommunityDto>> GetCommunitiesByUserAsync(int userId);

        // 5. UNIRSE A COMUNIDAD
        Task JoinCommunityAsync(int communityId, int userId);

        // 6. SALIR DE COMUNIDAD
        Task LeaveCommunityAsync(int communityId, int userId);
    }
}
