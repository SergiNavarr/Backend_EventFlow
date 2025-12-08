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

            return await ConvertListToDto(posts, currentUserId);
        }

        // 3. OBTENER POST POR ID
        public async Task<PostDto> GetPostById(int id, int currentUserId)
        {
            var post = await _context.Posts
                .Include(p => p.Author)      // Necesario para nombre y foto
                .Include(p => p.Community)   // Necesario para saber contexto
                .Include(p => p.Event)       // Necesario para saber contexto
                .Include(p => p.Likes)       // CRÍTICO: Necesario para IsLikedByCurrentUser
                .Include(p => p.Comments)    // Necesario para CommentsCount
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive); // Filtramos por ID y que no esté borrado

            // Si no existe, devolvemos null (el controlador se encargará de mandar 404 NotFound)
            if (post == null)
            {
                return null;
            }

            // Reutilizamos tu método auxiliar de conversión (singular)
            return ConvertToDto(post, currentUserId);
        }

        // 3. POSTS DE UNA COMUNIDAD
        public async Task<List<PostDto>> GetPostsByCommunity(int communityId, int currentUserId)
        {
            var posts = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Community)
                .Include(p => p.Event)
                .Include(p => p.Event)
                .Include(p => p.Likes)
                .Where(p => p.IsActive && p.CommunityId == communityId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return await ConvertListToDto(posts, currentUserId);
        }

        // 4. POSTS DE UN EVENTO
        public async Task<List<PostDto>> GetPostsByEvent(int eventId, int currentUserId)
        {
            var posts = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Community)
                .Include(p => p.Event)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.IsActive && p.EventId == eventId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return await ConvertListToDto(posts, currentUserId);
        }

        // 5. TOGGLE LIKE
        public async Task<bool> ToggleLike(int postId, int userId)
        {
            // 1. Verificar si el post existe
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists) throw new Exception("El post no existe.");

            // 2. Buscar si ya existe el like (Clave compuesta: PostId + UserId)
            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                // A. SI YA EXISTE -> LO QUITAMOS (Dislike)
                _context.PostLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false; // Indicamos que ahora NO tiene like
            }
            else
            {
                // B. NO EXISTE -> LO CREAMOS (Like)
                var newLike = new PostLike
                {
                    PostId = postId,
                    UserId = userId,
                    LikedAt = DateTime.UtcNow
                };
                _context.PostLikes.Add(newLike);
                await _context.SaveChangesAsync();
                return true; // Indicamos que AHORA TIENE like
            }
        }

        // 6. COMENTARIOS
        public async Task<CommentDto> AddComment(int postId, CreateCommentDto dto, int userId)
        {
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists) throw new Exception("Post no encontrado");

            var comment = new Comment
            {
                PostId = postId,
                AuthorId = userId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var author = await _context.Users.FindAsync(userId);

            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                AuthorId = userId,
                AuthorName = author.Username,
                AuthorAvatar = author.AvatarUrl
            };
        }
        public async Task<List<CommentDto>> GetComments(int postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId && c.IsActive)
                .Include(c => c.Author)
                .OrderBy(c => c.CreatedAt) // Los comentarios viejos primero (tipo chat)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    AuthorId = c.AuthorId,
                    AuthorName = c.Author.Username,
                    AuthorAvatar = c.Author.AvatarUrl
                })
                .ToListAsync();
        }
        //


        // --- MÉTODOS PRIVADOS DE AYUDA ---

        private PostDto ConvertToDto(Post post, int currentUserId)
        {
            return new PostDto
            {
                // Datos básicos
                Id = post.Id,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,

                // Datos del Autor
                AuthorId = post.AuthorId,
                // Asumiendo que tu entidad User tiene 'Name' o 'UserName'
                AuthorName = post.Author?.Username ?? "Usuario Desconocido",
                // Asumiendo que tu entidad User tiene 'ProfileImageUrl' o similar
                AuthorAvatar = post.Author?.AvatarUrl,

                // Contexto (Nullables)
                CommunityId = post.CommunityId,
                CommunityName = post.Community?.Name,
                EventId = post.EventId,
                EventTitle = post.Event?.Title,

                // Contadores (Manejo de nulos seguro con ?.)
                LikesCount = post.Likes?.Count ?? 0,
                CommentsCount = post.Comments?.Count ?? 0,

                // Estado del usuario actual
                // Verifica si en la lista de Likes existe alguno del usuario actual
                IsLikedByMe = post.Likes != null && post.Likes.Any(l => l.UserId == currentUserId)
            };
        }
        private async Task<List<PostDto>> ConvertListToDto(List<Post> posts, int currentUserId)
        {
            // 1. OPTIMIZACIÓN: "El Truco de la Bolsa"
            // En lugar de preguntar post por post (lo cual sería lentísimo),
            // le pedimos a la base de datos TODOS los IDs de posts que este usuario likeó de una sola vez.
            var myLikedPostIds = await _context.PostLikes
                .Where(l => l.UserId == currentUserId)
                .Select(l => l.PostId)
                .ToListAsync();

            // 2. Mapeamos usando la lista que obtuvimos
            var dtoList = posts.Select(p =>
            {
                // Usamos el MapToDto que ya tenías (el simple)
                var dto = MapToDto(p);

                // AHORA le inyectamos la inteligencia social:
                // Si el ID de este post está en mi "bolsa" de likes, entonces es TRUE.
                dto.IsLikedByMe = myLikedPostIds.Contains(p.Id);

                // Opcional: Si quieres contadores reales (aunque cuidado con el rendimiento aquí)
                // Para MVP está bien hacerlo así, para PRO se deberían guardar contadores en la tabla Posts.
                dto.LikesCount = _context.PostLikes.Count(l => l.PostId == p.Id);
                dto.CommentsCount = _context.Comments.Count(c => c.PostId == p.Id);

                return dto;
            }).ToList();

            return dtoList;
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
