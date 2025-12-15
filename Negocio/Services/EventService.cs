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
            // Validación de Fechas: Inicio no puede ser después del Fin
            if (dto.EndDateTime.HasValue && dto.StartDateTime > dto.EndDateTime.Value)
            {
                throw new Exception("La fecha de inicio no puede ser posterior a la fecha de fin.");
            }

            // Validar que la fecha no sea en el pasado 
            if (dto.StartDateTime < DateTime.UtcNow)
            {
                throw new Exception("No puedes crear eventos en el pasado.");
            }

            //Validar comunidad si es que se proporciona
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
                CommunityId = dto.CommunityId, // Puede ser null
                OrganizerId = organizerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            // Buscamos el nombre del organizador para el DTO
            var organizer = await _context.Users.FindAsync(organizerId);

            // Retornamos el DTO
            return MapToDto(newEvent, organizer.Username, communityName: null, attendeesCount: 0, myStatus: null);
        }

        // 2. OBTENER EVENTO POR ID
        public async Task<EventDto> GetEventByIdAsync(int eventId, int currentUserId)
        {
            var evt = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Community)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive);

            if (evt == null) throw new Exception("Evento no encontrado.");

            // Contar asistentes totales
            int attendeesCount = await _context.EventAttendees.CountAsync(ea => ea.EventId == eventId);

            // Verificar si YO (currentUserId) estoy asistiendo
            // Buscamos en la tabla intermedia si existe la relación
            bool isAttending = await _context.EventAttendees
                .AnyAsync(ea => ea.EventId == eventId && ea.UserId == currentUserId);

            string? myStatus = isAttending ? "Going" : null;

            return MapToDto(evt, evt.Organizer.Username, evt.Community?.Name, attendeesCount, myStatus);
        }

        // 3. BUSCAR / LISTAR EVENTOS
        public async Task<List<EventDto>> SearchEventsAsync(string? searchTerm)
        {
            var query = _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Community)
                .Where(e => e.IsActive); // Solo activos

            // Filtro por término de búsqueda (Título o Descripción)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                string term = searchTerm.ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(term) ||
                                         e.Description.ToLower().Contains(term));
            }

            var events = await query.OrderBy(e => e.StartDateTime).ToListAsync();

            var result = new List<EventDto>();
            foreach (var evt in events)
            {
                int count = await _context.EventAttendees.CountAsync(ea => ea.EventId == evt.Id);

                result.Add(MapToDto(evt, evt.Organizer.Username, evt.Community?.Name, count, null));
            }

            return result;
        }

        // 4. UNIRSE (JOIN) 
        public async Task JoinEventAsync(int eventId, int userId)
        {
            var evt = await _context.Events.FindAsync(eventId);
            if (evt == null || !evt.IsActive ) throw new Exception("Evento no encontrado.");

            // Verificar si ya estoy unido
            bool alreadyJoined = await _context.EventAttendees
                .AnyAsync(ea => ea.EventId == eventId && ea.UserId == userId);

            if (alreadyJoined) return; // Si ya estoy, no hago nada 

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

            // Contamos asistentes para devolver el DTO bien formado
            int attendeesCount = await _context.EventAttendees.CountAsync(ea => ea.EventId == eventId);

            // Retornamos el DTO actualizado
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

            // Soft Delete (Lo desactivamos en lugar de borrarlo físicamente)
            evt.IsActive = false;
            evt.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // Obtener lista de eventos a los que Asisto
        public async Task<List<EventDto>> GetMyCalendarEventsAsync(int userId)
        {
            // 1. Obtener IDs de las comunidades a las que pertenezco
            var myCommunityIds = await _context.UserCommunities
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CommunityId)
                .ToListAsync();

            // 2. Query Principal: Traer eventos que cumplan Condición A o Condición B
            var events = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Community)
                .Where(e => e.IsActive && (
                    // A. Soy asistente (estoy en la tabla EventAttendees)
                    e.Attendees.Any(a => a.UserId == userId) ||

                    // B. Es un evento de una de mis comunidades
                    (e.CommunityId.HasValue && myCommunityIds.Contains(e.CommunityId.Value))
                ))
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            // 3. Optimización: Traer mis asistencias reales para saber cuál es cuál
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
