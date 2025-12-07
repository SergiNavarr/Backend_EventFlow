using Datos.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Interfaces
{
    public interface IPostService
    {
        Task<PostDto> CreatePost(CreatePostDto dto, int userId);

        // Obtiene todos los posts (Feed Global)
        Task<List<PostDto>> GetAllPosts(int currentUserId);

        // Obtiene posts de una comunidad específica
        Task<List<PostDto>> GetPostsByCommunity(int communityId, int currentUserId);

        // Posts referidos a un Evento específico
        Task<List<PostDto>> GetPostsByEvent(int eventId, int currentUserId);
    }
}
