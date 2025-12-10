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

        //Obtiene un post por su id
        Task<PostDto> GetPostById(int id, int currentUserId);

        // Obtiene posts de una comunidad específica
        Task<List<PostDto>> GetPostsByCommunity(int communityId, int currentUserId);

        // Posts referidos a un Evento específico
        Task<List<PostDto>> GetPostsByEvent(int eventId, int currentUserId);

        // Alterna el "like" de un usuario en un post
        Task<bool> ToggleLike(int postId, int userId);

        // Comentarios
        Task<CommentDto> AddComment(int postId, CreateCommentDto dto, int userId);
        Task<List<CommentDto>> GetComments(int postId);

        // Actualizar Post
        Task<PostDto> UpdatePost(int postId, UpdatePostDto dto, int userId);

        // Borrar Post (Soft Delete)
        Task DeletePost(int postId, int userId);

    }
}
