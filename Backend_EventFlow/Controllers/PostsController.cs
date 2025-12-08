using Datos.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Interfaces;
using System.Security.Claims;

namespace Backend_EventFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        // POST: api/posts
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePostDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var post = await _postService.CreatePost(dto, userId);
                return CreatedAtAction(nameof(GetAll), new { id = post.Id }, post);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/posts
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var posts = await _postService.GetAllPosts(userId);
            return Ok(posts);
        }

        // GET: api/posts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Obtenemos el usuario actual para saber si le dio like o es autor
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Aquí buscas UN solo post por su ID primaria
            var post = await _postService.GetPostById(id, userId);

            if (post == null)
            {
                return NotFound();
            }

            return Ok(post);
        }

        // GET: api/posts/community/{id}
        [HttpGet("community/{communityId}")]
        public async Task<IActionResult> GetByCommunity(int communityId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var posts = await _postService.GetPostsByCommunity(communityId, userId);
            return Ok(posts);
        }

        // GET: api/posts/event/{id}
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var posts = await _postService.GetPostsByEvent(eventId, userId);
            return Ok(posts);
        }

        // POST: api/posts/{id}/like
        [HttpPost("{id}/like")]
        public async Task<IActionResult> ToggleLike(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                bool isLiked = await _postService.ToggleLike(id, userId);

                return Ok(new
                {
                    message = isLiked ? "Like agregado" : "Like eliminado",
                    isLiked = isLiked
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/posts/{id}/comments
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] CreateCommentDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var comment = await _postService.AddComment(id, dto, userId);
                return Ok(comment);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // GET: api/posts/{id}/comments
        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetComments(int id)
        {
            var comments = await _postService.GetComments(id);
            return Ok(comments);
        }
    }
}
