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

        //5.UNIRSE A COMUNIDAD
        // POST: api/communities/5/join
        [HttpPost("{id}/join")]
        public async Task<IActionResult> Join(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                await _communityService.JoinCommunityAsync(id, userId);

                return Ok(new { message = "Te has unido a la comunidad exitosamente." });
            }
            catch (Exception ex)
            {
                // Devolvemos 400 Bad Request si ya era miembro
                return BadRequest(new { message = ex.Message });
            }
        }

        //6. SALIR DE COMUNIDAD
        // POST: api/communities/5/leave
        [HttpPost("{id}/leave")]
        public async Task<IActionResult> Leave(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                await _communityService.LeaveCommunityAsync(id, userId);

                return Ok(new { message = "Has salido de la comunidad." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //7. ACTUALIZAR COMUNIDAD
        // PUT: api/communities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommunityDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var result = await _communityService.UpdateCommunityAsync(id, dto, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //8. ELIMINAR COMUNIDAD
        // DELETE: api/communities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                await _communityService.DeleteCommunityAsync(id, userId);

                return Ok(new { message = "Comunidad eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
