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
    public class EventService : IEventService
    {
        private readonly EventflowDbContext _context;

        public EventService(EventflowDbContext context)
        {
            _context = context;
        }

        // 1. CREAR EVENTO
        public async Task<EventDto> CreateEventAsync(CreateEventDto dto, int organizerId)
        {
            if (dto.EndDateTime.HasValue && dto.StartDateTime > dto.EndDateTime.Value)
            {
                throw new Exception("La fecha de inicio no puede ser posterior a la fecha de fin.");
            }

            if (dto.StartDateTime < DateTime.UtcNow)
            {
                throw new Exception("No puedes crear eventos en el pasado.");
            }

            if (dto.CommunityId.HasValue)
            {
                bool communityExists = await _context.Communities
                    .AnyAsync(c => c.Id == dto.CommunityId && c.IsActive);

                if (!communityExists)
                    throw new Exception("No puedes crear un evento en una comunidad eliminada.");
            }

            var newEvent = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                Location = dto.Location,
                IsOnline = dto.IsOnline,
                CoverImageUrl = dto.CoverImageUrl,
                MaxAttendees = dto.MaxAttendees,
                CommunityId = dto.CommunityId,
                OrganizerId = organizerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();


            var organizer = await _context.Users.FindAsync(organizerId);

            return MapToDto(newEvent, organizer.Username, communityName: null, attendeesCount: 0, myStatus: null);
        }

        // 2. OBTENER EVENTO POR ID
        public async Task<EventDto> GetEventByIdAsync(int eventId, int? currentUserId = null)
        {
            var evt = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Community)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive);

            if (evt == null) throw new Exception("Evento no encontrado.");

            int attendeesCount = await _context.EventAttendees.CountAsync(ea => ea.EventId == eventId);

            string? myStatus = null;

            // Solo consultamos a la DB si el usuario REALMENTE existe 
            if (currentUserId.HasValue)
            {
                bool isAttending = await _context.EventAttendees
                    .AnyAsync(ea => ea.EventId == eventId && ea.UserId == currentUserId.Value);

                if (isAttending) myStatus = "Going";
            }

            return MapToDto(evt, evt.Organizer.Username, evt.Community?.Name, attendeesCount, myStatus);
        }

        // 3. BUSCAR / LISTAR EVENTOS
        public async Task<List<EventDto>> SearchEvents(string query, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<EventDto>();

            string term = query.ToLower();

            var events = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Attendees)
                .Where(e => e.IsActive &&
                           (e.Title.ToLower().Contains(term) || e.Description.ToLower().Contains(term)))
                .OrderByDescending(e => e.StartDateTime)
                .Take(20)
                .ToListAsync();


            return events.Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartDateTime = e.StartDateTime,
                EndDateTime = e.EndDateTime,
                Location = e.Location,
                IsOnline = e.IsOnline,
                CoverImageUrl = e.CoverImageUrl,

                OrganizerId = e.OrganizerId,
                OrganizerName = e.Organizer.Username,
                OrganizerAvatar = e.Organizer.AvatarUrl,

                AttendeesCount = e.Attendees.Count,

                MyRsvpStatus = e.Attendees
                    .FirstOrDefault(a => a.UserId == currentUserId)?.RSVPStatus.ToString() ?? "NotGoing"
            }).ToList();
        }

        // 4. UNIRSE (JOIN) 
        public async Task JoinEventAsync(int eventId, int userId)
        {
            var evt = await _context.Events.FindAsync(eventId);
            if (evt == null || !evt.IsActive ) throw new Exception("Evento no encontrado.");

            // Verificar si ya estoy unido
            bool alreadyJoined = await _context.EventAttendees
                .AnyAsync(ea => ea.EventId == eventId && ea.UserId == userId);

            if (alreadyJoined) return;

            // Verificar Cupos (MaxAttendees)
            if (evt.MaxAttendees.HasValue)
            {
                int currentCount = await _context.EventAttendees.CountAsync(ea => ea.EventId == eventId);
                if (currentCount >= evt.MaxAttendees.Value)
                {
                    throw new Exception("El evento ha alcanzado su capacidad máxima.");
                }
            }

            // Crear la relación
            var attendee = new EventAttendee
            {
                EventId = eventId,
                UserId = userId,
                RSVPStatus = "Going", 
                ResponseDate = DateTime.UtcNow
            };

            _context.EventAttendees.Add(attendee);
            await _context.SaveChangesAsync();
        }

        // 5. SALIRSE (LEAVE)
        public async Task LeaveEventAsync(int eventId, int userId)
        {
            var attendee = await _context.EventAttendees
                .FirstOrDefaultAsync(ea => ea.EventId == eventId && ea.UserId == userId);

            if (attendee != null)
            {
                _context.EventAttendees.Remove(attendee);
                await _context.SaveChangesAsync();
            }
        }

        // Actualizar evento
        public async Task<EventDto> UpdateEventAsync(int eventId, UpdateEventDto dto, int userId)
        {
            var evt = await _context.Events
                .Include(e => e.Organizer) 
                .Include(e => e.Community)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (evt == null) throw new Exception("Evento no encontrado.");

            // VALIDACIÓN DE DUEÑO
            if (evt.OrganizerId != userId)
            {
                throw new Exception("No tienes permiso para modificar este evento.");
            }

            // Validar fechas
            if (dto.EndDateTime.HasValue && dto.StartDateTime > dto.EndDateTime.Value)
            {
                throw new Exception("La fecha de inicio no puede ser posterior al fin.");
            }

            // Actualizamos campos
            evt.Title = dto.Title;
            evt.Description = dto.Description;
            evt.StartDateTime = dto.StartDateTime;
            evt.EndDateTime = dto.EndDateTime;
            evt.Location = dto.Location;
            evt.IsOnline = dto.IsOnline;
            evt.CoverImageUrl = dto.CoverImageUrl;
            evt.MaxAttendees = dto.MaxAttendees;
            evt.UpdatedAt = DateTime.UtcNow; 

            await _context.SaveChangesAsync();

            int attendeesCount = await _context.EventAttendees.CountAsync(ea => ea.EventId == eventId);

            return MapToDto(evt, evt.Organizer.Username, evt.Community?.Name, attendeesCount, null);
        }

        //Borrar evento
        public async Task DeleteEventAsync(int eventId, int userId)
        {
            var evt = await _context.Events.FindAsync(eventId);

            if (evt == null) throw new Exception("Evento no encontrado.");

            // VALIDACIÓN DE DUEÑO
            if (evt.OrganizerId != userId)
            {
                throw new Exception("No tienes permiso para eliminar este evento.");
            }

            evt.IsActive = false;
            evt.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // MIS EVENTOS CREADOS
        public async Task<List<EventDto>> GetMyCreatedEvents(int userId)
        {
            var events = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Community)
                .Where(e => e.IsActive && e.OrganizerId == userId) 
                .OrderByDescending(e => e.StartDateTime)
                .ToListAsync();

            // Reutilizamos lógica de conversión
            var result = new List<EventDto>();
            foreach (var evt in events)
            {
                int count = await _context.EventAttendees.CountAsync(ea => ea.EventId == evt.Id);
              
                bool isJoined = await _context.EventAttendees.AnyAsync(ea => ea.EventId == evt.Id && ea.UserId == userId);

                result.Add(MapToDto(evt, evt.Organizer.Username, evt.Community?.Name, count, isJoined ? "Going" : null));
            }
            return result;
        }

        // Obtener lista de eventos a los que Asisto
        public async Task<List<EventDto>> GetMyCalendarEventsAsync(int userId)
        {
            var myCommunityIds = await _context.UserCommunities
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CommunityId)
                .ToListAsync();

            var events = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Community)
                .Where(e => e.IsActive && (
                    e.Attendees.Any(a => a.UserId == userId) ||

                    (e.CommunityId.HasValue && myCommunityIds.Contains(e.CommunityId.Value))
                ))
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            var myAttendanceList = await _context.EventAttendees
                .Where(ea => ea.UserId == userId)
                .Select(ea => ea.EventId)
                .ToListAsync();

            var myAttendanceIds = myAttendanceList.ToHashSet();

            var dtoList = new List<EventDto>();

            foreach (var evt in events)
            {
                // Contar asistentes totales
                int attendeesCount = await _context.EventAttendees.CountAsync(ea => ea.EventId == evt.Id);

                // Determinamos el estado: 
                string? myStatus = myAttendanceIds.Contains(evt.Id) ? "Going" : null;

                dtoList.Add(MapToDto(evt, evt.Organizer.Username, evt.Community?.Name, attendeesCount, myStatus));
            }

            return dtoList;
        }

        // --- Helper Privado ---
        private EventDto MapToDto(Event e, string orgName, string? communityName, int attendeesCount, string? myStatus)
        {
            return new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartDateTime = e.StartDateTime,
                EndDateTime = e.EndDateTime,
                Location = e.Location,
                IsOnline = e.IsOnline,
                CoverImageUrl = e.CoverImageUrl,
                MaxAttendees = e.MaxAttendees,
                OrganizerId = e.OrganizerId,
                OrganizerName = orgName,
                CommunityId = e.CommunityId,
                CommunityName = communityName,
                AttendeesCount = attendeesCount,
                MyRsvpStatus = myStatus
            };
        }
    }
}
