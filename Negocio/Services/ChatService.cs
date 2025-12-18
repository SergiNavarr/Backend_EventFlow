using Datos.Data;
using Datos.DTOs;
using Datos.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Negocio.Interfaces;
using Negocio.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Services
{
    public class ChatService : IChatService
    {
        private readonly EventflowDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatService(EventflowDbContext context, IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
            _context = context;
        }

        public async Task<List<EventMessageDto>> GetEventMessages(int eventId, int currentUserId)
        {
            var exists = await _context.Events.AnyAsync(e => e.Id == eventId);
            if (!exists) throw new Exception("Evento no encontrado.");

            var messages = await _context.EventChatMessages
                .Include(m => m.Sender)
                .Where(m => m.EventId == eventId && m.IsActive)
                .OrderBy(m => m.CreatedAt) // Orden cronológico (chat)
                .Select(m => new EventMessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.Username,
                    SenderAvatar = m.Sender.AvatarUrl,
                    IsMine = m.SenderId == currentUserId
                })
                .ToListAsync();

            return messages;
        }

        public async Task<EventMessageDto> SendEventMessage(int eventId, int userId, string content)
        {
            var eventData = await _context.Events
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventData == null) throw new Exception("Evento no encontrado.");

            bool isOrganizer = eventData.OrganizerId == userId;
            bool isAttendee = eventData.Attendees.Any(a => a.UserId == userId);

            if (!isOrganizer && !isAttendee)
            {
                throw new Exception("No puedes enviar mensajes si no estás unido al evento.");
            }

            var msg = new EventChatMessage
            {
                EventId = eventId,
                SenderId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.EventChatMessages.Add(msg);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync(userId);

            var messageDto = new EventMessageDto
            {
                Id = msg.Id,
                Content = msg.Content,
                CreatedAt = msg.CreatedAt,
                SenderId = userId,
                SenderName = sender.Username,
                SenderAvatar = sender.AvatarUrl,
                IsMine = false //Al enviarlo por socket, para los otros NO es "Mine"
            };

            await _hubContext.Clients.Group(eventId.ToString())
                .SendAsync("ReceiveMessage", messageDto);

            messageDto.IsMine = true;
            return messageDto;
        }
    }
}
