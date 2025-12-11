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
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatsController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // GET: api/chats/event/{eventId}
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventChat(int eventId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var messages = await _chatService.GetEventMessages(eventId, userId);
                return Ok(messages);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // POST: api/chats/event/{eventId}
        [HttpPost("event/{eventId}")]
        public async Task<IActionResult> SendEventMessage(int eventId, [FromBody] CreateEventMessageDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var message = await _chatService.SendEventMessage(eventId, userId, dto.Content);
                return Ok(message);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
