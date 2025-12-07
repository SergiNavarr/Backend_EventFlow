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
            // Validar si ya existe una con ese nombre
            if (await _context.Communities.AnyAsync(c => c.Name == dto.Name))
            {
                throw new Exception("Ya existe una comunidad con este nombre.");
            }

            // Mapeo Manual: DTO -> Entidad
            var community = new Community
            {
                Name = dto.Name,
                Description = dto.Description,
                CoverImageUrl = dto.CoverImageUrl,
                OwnerId = userId, // Asignamos al usuario logueado como dueño
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Communities.Add(community);
            await _context.SaveChangesAsync();

            // Obtenemos el nombre del dueño para devolverlo en el DTO
            var ownerName = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Username)
                .FirstOrDefaultAsync();

            // Devolvemos el DTO
            return MapToDto(community, ownerName ?? "Desconocido", 0);
        }

        // 2. LISTAR TODAS
        public async Task<List<CommunityDto>> GetAllCommunitiesAsync()
        {
            // Traemos las comunidades activas e INCLUIMOS los datos del Owner
            var communities = await _context.Communities
                .Include(c => c.Owner)
                .Where(c => c.IsActive)
                .ToListAsync();

            var dtoList = new List<CommunityDto>();

            foreach (var c in communities)
            {
                // Contamos cuántos miembros tiene (consultando la tabla intermedia)
                int members = await _context.UserCommunities.CountAsync(uc => uc.CommunityId == c.Id);

                dtoList.Add(MapToDto(c, c.Owner.Username, members));
            }

            return dtoList;
        }

        // 3. OBTENER POR ID
        public async Task<CommunityDto> GetByIdAsync(int id)
        {
            var community = await _context.Communities
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (community == null)
            {
                throw new Exception("Comunidad no encontrada.");
            }

            int members = await _context.UserCommunities.CountAsync(uc => uc.CommunityId == id);

            return MapToDto(community, community.Owner.Username, members);
        }

        // 4. MIS COMUNIDADES (Creadas por mí)
        public async Task<List<CommunityDto>> GetCommunitiesByUserAsync(int userId)
        {
            var communities = await _context.Communities
                .Include(c => c.Owner)
                .Where(c => c.OwnerId == userId && c.IsActive)
                .ToListAsync();

            var dtoList = new List<CommunityDto>();

            foreach (var c in communities)
            {
                int members = await _context.UserCommunities.CountAsync(uc => uc.CommunityId == c.Id);
                dtoList.Add(MapToDto(c, c.Owner.Username, members));
            }

            return dtoList;
        }

        // --- MÉTODO PRIVADO (Helper) ---
        // Usamos esto para no repetir el código de asignación (Entidad -> DTO) 4 veces
        private CommunityDto MapToDto(Community c, string ownerName, int memberCount)
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
                IsMember = false // Por ahora false, luego implementaremos esta lógica
            };
        }
    }
}
