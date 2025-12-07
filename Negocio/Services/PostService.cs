using Datos.Data;
using Datos.DTOs;
using Datos.Models;
using Microsoft.EntityFrameworkCore;
using Negocio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Services
{
    public class PostService : IPostService
    {
        private readonly EventflowDbContext _context;

        public PostService(EventflowDbContext context)
        {
            _context = context;
        }

        // 1. CREAR POST
        public async Task<PostDto> CreatePost(CreatePostDto dto, int userId)
        {
            // Validaciones: ¿Existe la comunidad? ¿Existe el evento?
            if (dto.CommunityId.HasValue && !await _context.Communities.AnyAsync(c => c.Id == dto.CommunityId))
                throw new Exception("La comunidad referenciada no existe.");

            if (dto.EventId.HasValue && !await _context.Events.AnyAsync(e => e.Id == dto.EventId))
                throw new Exception("El evento referenciado no existe.");

            var post = new Post
            {
                Content = dto.Content,
                ImageUrl = dto.ImageUrl,
                AuthorId = userId,
                CommunityId = dto.CommunityId,
                EventId = dto.EventId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Cargamos datos del autor para el DTO
            var loadedPost = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Community)
                .Include(p => p.Event)
                .FirstAsync(p => p.Id == post.Id);

            return MapToDto(loadedPost);
        }

        // 2. FEED GLOBAL (Últimos posts)
        public async Task<List<PostDto>> GetAllPosts(int currentUserId)
        {
            var posts = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Community)
                .Include(p => p.Event)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt) // Más nuevos primero
                .ToListAsync();

            return ConvertListToDto(posts);
        }

        // 3. POSTS DE UNA COMUNIDAD
        public async Task<List<PostDto>> GetPostsByCommunity(int communityId, int currentUserId)
        {
            var posts = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Community)
                .Include(p => p.Event)
                .Where(p => p.IsActive && p.CommunityId == communityId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return ConvertListToDto(posts);
        }

        // 4. POSTS DE UN EVENTO
        public async Task<List<PostDto>> GetPostsByEvent(int eventId, int currentUserId)
        {
            var posts = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Community)
                .Include(p => p.Event)
                .Where(p => p.IsActive && p.EventId == eventId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return ConvertListToDto(posts);
        }

        // --- MÉTODOS PRIVADOS DE AYUDA ---

        private List<PostDto> ConvertListToDto(List<Post> posts)
        {
            return posts.Select(p => MapToDto(p)).ToList();
        }

        private PostDto MapToDto(Post p)
        {
            return new PostDto
            {
                Id = p.Id,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,

                // Autor
                AuthorId = p.AuthorId,
                AuthorName = p.Author.Username,
                AuthorAvatar = p.Author.AvatarUrl,

                // Referencias
                CommunityId = p.CommunityId,
                CommunityName = p.Community?.Name,

                EventId = p.EventId,
                EventTitle = p.Event?.Title,

                // Contadores (Hardcodeados en 0 por ahora)
                LikesCount = 0,
                CommentsCount = 0,
                IsLikedByMe = false
            };
        }
    }
}
