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
            try 
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var posts = await _postService.GetPostsByCommunity(communityId, userId);
            return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/posts/event/{id}
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var posts = await _postService.GetPostsByEvent(eventId, userId);
            return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
            try
            {
                var comments = await _postService.GetComments(id);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/posts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePostDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var updatedPost = await _postService.UpdatePost(id, dto, userId);
                return Ok(updatedPost);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("permiso")) return StatusCode(403, new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/posts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await _postService.DeletePost(id, userId);
                return Ok(new { message = "Post eliminado correctamente." });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("permiso")) return StatusCode(403, new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/posts/feed?page=1&pageSize=10
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var posts = await _postService.GetHomeFeed(userId, page, pageSize);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/posts/my-posts
        [HttpGet("my-posts")]
        public async Task<IActionResult> GetMyPosts()
        {
            try
            {
                // Extraemos "quién soy yo" del Token
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                // Pedimos los posts donde el Autor soy YO (userId, userId)
                var posts = await _postService.GetPostsByAuthor(userId, userId);

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
