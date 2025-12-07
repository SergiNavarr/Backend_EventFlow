using Datos.Data;
using Datos.DTOs;
using Datos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Negocio.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Services
{
    public class UserService : IUserService
    {
        private readonly EventflowDbContext _context;
        private readonly IConfiguration _config;

        public UserService(EventflowDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task Register(UserRegisterDto dto)
        {
            //Validar Email duplicado
            if(await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                throw new Exception("El Email ya está registrado");
            }

            //Validar Username duplicado
            if(await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                throw new Exception("El nombre de usuario no está disponible");
            }

            // Hashear la contraseña
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Crear entidad 
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Communities = new List<UserCommunity>(),
                Followers = new List<UserFollow>(),
                Following = new List<UserFollow>()

            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<AuthResponseDto> Login(UserLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if(user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                throw new Exception("Credenciales inválidas");
            }

            //Generar Token
            string token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task<UserProfileDto> GetById(int userId)
        {
            // Buscamos al usuario
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                throw new Exception("Usuario no encontrado");

            // TRUCO DE RENDIMIENTO:
            // En lugar de cargar TODA la lista de seguidores (que podrían ser miles),
            // hacemos una consulta "Count" directa a la base de datos.

            var followersCount = await _context.UserFollows.CountAsync(f => f.FollowedId == userId);
            var followingCount = await _context.UserFollows.CountAsync(f => f.FollowerId == userId);

            // Mapeo de Dto
            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                FollowersCount = followersCount,
                FollowingCount = followingCount
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = _config["Jwt:Key"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
