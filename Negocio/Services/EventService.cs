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

            // Validar que la fecha no sea en el pasado (Opcional)
            if (dto.StartDateTime < DateTime.UtcNow)
            {
                throw new Exception("No puedes crear eventos en el pasado.");
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
                .FirstOrDefaultAsync(e => e.Id == eventId);

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
                // Nota: En listados masivos, a veces se evita contar uno por uno por rendimiento.
                // Aquí lo hacemos simple.
                int count = await _context.EventAttendees.CountAsync(ea => ea.EventId == evt.Id);

                // En el listado general, no calculamos "MyRsvpStatus" para ahorrar recursos
                result.Add(MapToDto(evt, evt.Organizer.Username, evt.Community?.Name, count, null));
            }

            return result;
        }

        // 4. UNIRSE (JOIN) - Lógica Opción B
        public async Task JoinEventAsync(int eventId, int userId)
        {
            var evt = await _context.Events.FindAsync(eventId);
            if (evt == null) throw new Exception("Evento no encontrado.");

            // A. Verificar si ya estoy unido
            bool alreadyJoined = await _context.EventAttendees
                .AnyAsync(ea => ea.EventId == eventId && ea.UserId == userId);

            if (alreadyJoined) return; // Si ya estoy, no hago nada (Idempotencia)

            // B. Verificar Cupos (MaxAttendees)
            if (evt.MaxAttendees.HasValue)
            {
                int currentCount = await _context.EventAttendees.CountAsync(ea => ea.EventId == eventId);
                if (currentCount >= evt.MaxAttendees.Value)
                {
                    throw new Exception("El evento ha alcanzado su capacidad máxima.");
                }
            }

            // C. Crear la relación
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
