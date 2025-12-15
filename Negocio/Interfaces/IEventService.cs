using Datos.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Interfaces
{
    public interface IEventService
    {
        // 1. Crear Evento
        Task<EventDto> CreateEventAsync(CreateEventDto dto, int organizerId);

        // 2. Obtener Evento por ID (Con detalles)
        // Aquí necesitamos el userId para saber si YO ya estoy unido al evento o no
        Task<EventDto> GetEventByIdAsync(int eventId, int currentUserId);

        // 3. Listar Eventos (Con filtros básicos)
        Task<List<EventDto>> SearchEventsAsync(string? searchTerm);

        // --- FUNCIONALIDAD SOCIAL---

        // Unirse al evento (Botón "Join")
        Task JoinEventAsync(int eventId, int userId);

        // Salirse del evento (Botón "Leave")
        Task LeaveEventAsync(int eventId, int userId);

        // Actualizar evento
        Task<EventDto> UpdateEventAsync(int eventId, UpdateEventDto dto, int userId);

        // Eliminar evento (soft delete)
        Task DeleteEventAsync(int eventId, int userId);

        // Mis Eventos Creados
        Task<List<EventDto>> GetMyCreatedEvents(int userId);

        // Obtener eventos del calendario del usuario
        Task<List<EventDto>> GetMyCalendarEventsAsync(int userId);
    }
}
