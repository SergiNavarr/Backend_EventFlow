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
    public class CommunityService : ICommunityService
    {
        private readonly EventflowDbContext _context;

        // Inyectamos la base de datos
        public CommunityService(EventflowDbContext context)
        {
            _context = context;
        }

        // 1. CREAR COMUNIDAD
        public async Task<CommunityDto> CreateCommunityAsync(CreateCommunityDto dto, int userId)
        {
            // 1. Validaciones previas
            if (await _context.Communities.AnyAsync(c => c.Name == dto.Name))
            {
                throw new Exception("Ya existe una comunidad con este nombre.");
            }

            // 2. Crear la Comunidad 
            var community = new Community
            {
                Name = dto.Name,
                Description = dto.Description,
                CoverImageUrl = dto.CoverImageUrl,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Communities.Add(community);

            await _context.SaveChangesAsync();

            // 3. Crear la Membresía 
            var membership = new UserCommunity
            {
                UserId = userId,
                CommunityId = community.Id, // Aquí usamos el ID recién generado
                Role = "Owner", // Le damos el rango máximo
                JoinedAt = DateTime.UtcNow
            };

            _context.UserCommunities.Add(membership);
            var autoPost = new Post
            {
                AuthorId = userId,

                // Contenido del post
                Content = $"¡He creado la comunidad \"{community.Name}\". ¡Únanse y participen!",

                CreatedAt = DateTime.UtcNow,
                IsActive = true,

                CommunityId = community.Id
            };

            _context.Posts.Add(autoPost);

            // Guardamos la Membresía Y el Post al mismo tiempo
            await _context.SaveChangesAsync();

            // 4. Retornar el DTO
            var ownerName = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Username)
                .FirstOrDefaultAsync();

            return MapToDto(community, ownerName ?? "Desconocido", 1, true);
        }

        // 2. LISTAR TODAS
        public async Task<List<CommunityDto>> GetAllCommunitiesAsync(int? currentUserId)
        {
            var communities = await _context.Communities
                .Include(c => c.Owner)
                .Where(c => c.IsActive)
                .ToListAsync();

            // OPTIMIZACIÓN: 
            // Traemos TODOS los IDs de comunidades donde el usuario actual es miembro en una sola consulta.
            // Esto evita hacer una consulta a la DB por cada comunidad en el bucle (Problema N+1).
            var joinedCommunityIds = new HashSet<int>();
            if (currentUserId.HasValue)
            {
                // Primero traemos la lista con ToListAsync (Asíncrono DB)
                var idsList = await _context.UserCommunities
                    .Where(uc => uc.UserId == currentUserId.Value)
                    .Select(uc => uc.CommunityId)
                    .ToListAsync();

                // Luego lo convertimos a HashSet en memoria (Síncrono)
                joinedCommunityIds = idsList.ToHashSet();
            }

            var dtoList = new List<CommunityDto>();

            foreach (var c in communities)
            {
                // Calcular miembros
                int members = await _context.UserCommunities.CountAsync(uc => uc.CommunityId == c.Id);

                // Verificamos si el ID de esta comunidad está en la lista de "Mis Comunidades"
                bool isMember = joinedCommunityIds.Contains(c.Id);

                dtoList.Add(MapToDto(c, c.Owner.Username, members, isMember));
            }

            return dtoList;
        }

        // 3. OBTENER POR ID
        public async Task<CommunityDto> GetByIdAsync(int communityId, int? currentUserId)
        {
            var community = await _context.Communities
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.Id == communityId && c.IsActive);

            if (community == null)
            {
                throw new Exception("Comunidad no encontrada.");
            }

            // 1. Contar miembros totales
            int members = await _context.UserCommunities.CountAsync(uc => uc.CommunityId == communityId);

            // 2. Calcular si EL usuario actual es miembro
            bool isMember = false;

            if (currentUserId.HasValue)
            {
                isMember = await _context.UserCommunities
                    .AnyAsync(uc => uc.CommunityId == communityId && uc.UserId == currentUserId.Value);
            }

            // 3. Pasar el dato al DTO
            return MapToDto(community, community.Owner.Username, members, isMember);
        }

        // 4. MIS COMUNIDADES (Creadas por mí)
        public async Task<List<CommunityDto>> GetCommunitiesByUserAsync(int targetUserId, int? viewerId)
        {
            // 'targetUserId' es el dueño de las comunidades que buscamos
            var communities = await _context.Communities
                .Include(c => c.Owner)
                .Where(c => c.OwnerId == targetUserId && c.IsActive)
                .ToListAsync();

            // OPTIMIZACIÓN: IDs de comunidades donde el VIEWER es miembro
            var joinedCommunityIds = new HashSet<int>();
            if (viewerId.HasValue)
            {
                var idsList = await _context.UserCommunities
                    .Where(uc => uc.UserId == viewerId.Value)
                    .Select(uc => uc.CommunityId)
                    .ToListAsync(); // Usamos ToListAsync

                joinedCommunityIds = idsList.ToHashSet(); // Convertimos en memoria
            }

            var dtoList = new List<CommunityDto>();

            foreach (var c in communities)
            {
                int members = await _context.UserCommunities.CountAsync(uc => uc.CommunityId == c.Id);

                // Calculamos si el que mira es miembro
                bool isMember = joinedCommunityIds.Contains(c.Id);

                dtoList.Add(MapToDto(c, c.Owner.Username, members, isMember));
            }

            return dtoList;
        }

        // --- MÉTODO PRIVADO (Helper) ---
        private CommunityDto MapToDto(Community c, string ownerName, int memberCount, bool isMember)
        {
            return new CommunityDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CoverImageUrl = c.CoverImageUrl,
                CreatedAt = c.CreatedAt,
                OwnerId = c.OwnerId,
                OwnerName = ownerName,
                MemberCount = memberCount,
                IsMember = isMember
            };
        }

        // 5. UNIRSE A COMUNIDAD
        public async Task JoinCommunityAsync(int communityId, int userId)
    {
        // A. Validar que la comunidad exista
        bool communityExists = await _context.Communities.AnyAsync(c => c.Id == communityId && c.IsActive);
        if (!communityExists) throw new Exception("Comunidad no encontrada.");

        // B. Validar si YA es miembro 
        bool isMember = await _context.UserCommunities
            .AnyAsync(uc => uc.CommunityId == communityId && uc.UserId == userId);

        if (isMember)
        {
            throw new Exception("Ya eres miembro de esta comunidad.");
        }

        // C. Crear la relación
        var userCommunity = new UserCommunity
        {
            UserId = userId,
            CommunityId = communityId,
            Role = "Member", // Rol por defecto
            JoinedAt = DateTime.UtcNow
        };

        _context.UserCommunities.Add(userCommunity);
        await _context.SaveChangesAsync();
    }

    // 6. ABANDONAR COMUNIDAD
    public async Task LeaveCommunityAsync(int communityId, int userId)
    {
        // Obtener la comunidad para ver quién es el dueño
        var community = await _context.Communities.FindAsync(communityId);
        if (community == null) throw new Exception("Comunidad no encontrada.");

        // El dueño no puede abandonar la comunidad
        if (community.OwnerId == userId)
        {
            throw new Exception("El creador no puede salir de la comunidad. Debes eliminarla o transferir la propiedad.");
        }

        // C. Buscar la relación para borrarla
        var memberRecord = await _context.UserCommunities
            .FirstOrDefaultAsync(uc => uc.CommunityId == communityId && uc.UserId == userId);

        if (memberRecord != null)
        {
            _context.UserCommunities.Remove(memberRecord);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new Exception("No eres miembro de esta comunidad.");
        }
    }

        // 7. ACTUALIZAR COMUNIDAD
        public async Task<CommunityDto> UpdateCommunityAsync(int id, UpdateCommunityDto dto, int userId)
        {
            var community = await _context.Communities
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (community == null) throw new Exception("Comunidad no encontrada.");

            // VALIDACIÓN DE DUEÑO
            if (community.OwnerId != userId)
            {
                throw new Exception("No tienes permiso para modificar esta comunidad.");
            }

            // Validación de Nombre Duplicado 
            // Si cambia el nombre, verificamos que el nuevo nombre no esté ocupado por OTRO
            if (community.Name != dto.Name)
            {
                bool nameExists = await _context.Communities.AnyAsync(c => c.Name == dto.Name);
                if (nameExists) throw new Exception("Ya existe una comunidad con ese nombre.");
            }

            // Actualizar datos
            community.Name = dto.Name;
            community.Description = dto.Description;
            community.CoverImageUrl = dto.CoverImageUrl;
            community.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Recalcular miembros para devolver el DTO
            int members = await _context.UserCommunities.CountAsync(uc => uc.CommunityId == id);

            return MapToDto(community, community.Owner.Username, members, true);
        }

        // 8. BORRAR COMUNIDAD 
        public async Task DeleteCommunityAsync(int id, int userId)
        {
            var community = await _context.Communities.FindAsync(id);

            if (community == null) throw new Exception("Comunidad no encontrada.");

            // VALIDACIÓN DE DUEÑO
            if (community.OwnerId != userId)
            {
                throw new Exception("No tienes permiso para eliminar esta comunidad.");
            }

            // SOFT DELETE (Borrado Lógico)
            community.IsActive = false;
            community.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // 9. BUSCAR COMUNIDADES
        public async Task<List<CommunityDto>> SearchCommunities(string query, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<CommunityDto>();

            string term = query.ToLower();

            var communities = await _context.Communities
                .Include(c => c.Owner)
                .Where(c => c.IsActive &&
                       (c.Name.ToLower().Contains(term) || c.Description.ToLower().Contains(term)))
                .Take(20)
                .ToListAsync();

            var dtoList = new List<CommunityDto>();

            foreach (var c in communities)
            {
                int memberCount = await _context.UserCommunities
                    .CountAsync(uc => uc.CommunityId == c.Id);

                bool isMember = await _context.UserCommunities
                    .AnyAsync(uc => uc.CommunityId == c.Id && uc.UserId == currentUserId);

                dtoList.Add(MapToDto(c, c.Owner.Username, memberCount, isMember));
            }

            return dtoList;
        }
    }
}
