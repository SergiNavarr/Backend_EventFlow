using Datos.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Services;
using Negocio.Interfaces;
using System.Security.Claims; 

namespace Backend_Eventflow.Controllers
{
    [Route("api/[controller]")] // Ruta: api/events
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
                // Obtenemos el ID del organizador (usuario logueado)
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
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                // Pasamos el userId para saber si el usuario YA está unido (MyRsvpStatus)
                var evt = await _eventService.GetEventByIdAsync(id, userId);
                return Ok(evt);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // 3. BUSCAR / LISTAR
        // GET: api/events?search=curso
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string? search)
        {
            var events = await _eventService.SearchEventsAsync(search);
            return Ok(events);
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
    }
}