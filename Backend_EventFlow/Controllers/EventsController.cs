using Datos.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Services;
using Negocio.Interfaces;
using System.Security.Claims; 

namespace Backend_Eventflow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        // 1. CREAR EVENTO
        // POST: api/events
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            try
            {
                // Obtenemos el ID del organizador
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var createdEvent = await _eventService.CreateEventAsync(dto, userId);

                return CreatedAtAction(nameof(GetById), new { id = createdEvent.Id }, createdEvent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2. OBTENER POR ID (Detalle)
        // GET: api/events/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                int? currentUserId = null;

                var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

                if (claimId != null && int.TryParse(claimId.Value, out int parsedId))
                {
                    currentUserId = parsedId;
                }

                var evt = await _eventService.GetEventByIdAsync(id, currentUserId);

                return Ok(evt);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no encontrado")) return NotFound(new { message = ex.Message });
                return StatusCode(500, new { message = "Error interno" });
            }
        }

        // 3. BUSCAR
        // GET: api/events/search?query=concierto
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var results = await _eventService.SearchEvents(query, userId);
            return Ok(results);
        }

        // 4. UNIRSE (JOIN)
        // POST: api/events/5/join
        [HttpPost("{id}/join")]
        public async Task<IActionResult> Join(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                await _eventService.JoinEventAsync(id, userId);

                return Ok(new { message = "Te has unido al evento exitosamente." });
            }
            catch (Exception ex)
            {
                // Puede fallar si el evento está lleno o no existe
                return BadRequest(new { message = ex.Message });
            }
        }

        // 5. SALIRSE (LEAVE)
        // POST: api/events/5/leave
        [HttpPost("{id}/leave")]
        public async Task<IActionResult> Leave(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                await _eventService.LeaveEventAsync(id, userId);

                return Ok(new { message = "Has salido del evento." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 6. ACTUALIZAR EVENTO
        // PUT: api/events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
        {
            try
            {
                // Sacamos el ID del usuario del Token
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var updatedEvent = await _eventService.UpdateEventAsync(id, dto, userId);

                return Ok(updatedEvent);
            }
            catch (Exception ex)
            {
                // Si el error es "No tienes permiso", deberíamos devolver 403, 
                // pero por simplicidad usaremos BadRequest 
                if (ex.Message.Contains("permiso")) return StatusCode(403, new { message = ex.Message });

                return BadRequest(new { message = ex.Message });
            }
        }

        //7. ELIMINAR EVENTO
        // DELETE: api/events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                await _eventService.DeleteEventAsync(id, userId);

                return Ok(new { message = "Evento eliminado correctamente." });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("permiso")) return StatusCode(403, new { message = ex.Message });

                return BadRequest(new { message = ex.Message });
            }
        }

        // 8. MIS CREADOS
        // GET: api/events/my-created
        [HttpGet("my-created")]
        public async Task<IActionResult> GetMyCreated()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var events = await _eventService.GetMyCreatedEvents(userId);
                return Ok(events);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        //9. OBTENER EVENTOS DEL CALENDARIO DEL USUARIO
        // GET: api/events/calendar
        [HttpGet("calendar")]
        public async Task<IActionResult> GetMyCalendar()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var events = await _eventService.GetMyCalendarEventsAsync(userId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}