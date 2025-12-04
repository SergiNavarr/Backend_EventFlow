using Datos.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Interfaces;
using Negocio.Services;
using System.Security.Claims;

namespace Backend_EventFlow.Controllers
{
    [Route("api/[controller]")] // Ruta: api/communities
    [ApiController]
    [Authorize]
    public class CommunitiesController : ControllerBase
    {
        private readonly ICommunityService _communityService;

        public CommunitiesController(ICommunityService communityService)
        {
            _communityService = communityService;
        }

        // 1. CREAR COMUNIDAD
        // POST: api/communities
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommunityDto dto)
        {
            try
            {
                // A. Obtenemos el ID del usuario desde el Token
                // Esto es 100% seguro, el usuario no puede falsificarlo
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null) return Unauthorized(); // Por si acaso

                int userId = int.Parse(userIdClaim.Value);

                // B. Llamamos al servicio
                var createdCommunity = await _communityService.CreateCommunityAsync(dto, userId);

                // C. Retornamos 201 Created
                // "nameof(GetById)" le dice al cliente dónde puede consultar la comunidad creada
                return CreatedAtAction(nameof(GetById), new { id = createdCommunity.Id }, createdCommunity);
            }
            catch (Exception ex)
            {
                // Si falla (ej: nombre duplicado), devolvemos 400 Bad Request
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2. LISTAR TODAS (Explorar)
        // GET: api/communities
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _communityService.GetAllCommunitiesAsync();
            return Ok(list);
        }

        // 3. VER DETALLE
        // GET: api/communities/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var community = await _communityService.GetByIdAsync(id);
                return Ok(community);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // 4. MIS COMUNIDADES
        // GET: api/communities/my-communities
        [HttpGet("my-communities")]
        public async Task<IActionResult> GetMyCommunities()
        {
            // Sacamos el ID del token de nuevo
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var list = await _communityService.GetCommunitiesByUserAsync(userId);
            return Ok(list);
        }
    }
}
