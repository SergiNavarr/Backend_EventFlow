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

        public async Task<UserProfileDto> UpdateUser(int userId, UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive) throw new Exception("Usuario no encontrado.");

            // Solo actualizamos si el campo viene con datos (no es null)
            if (dto.Bio != null) user.Bio = dto.Bio;
            if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Devolvemos el perfil actualizado reutilizando tu método GetById 
            // (o mapeando manualmente si prefieres evitar la doble consulta)
            return await GetById(userId);
        }

        public async Task DeleteUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("Usuario no encontrado.");

            // Borrado Lógico
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;


            await _context.SaveChangesAsync();
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

        public async Task ChangePassword(int userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new Exception("Usuario no encontrado");
            }
            // Usamos BCrypt para comparar el texto plano con el hash guardado
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                throw new Exception("La contraseña actual es incorrecta");
            }

            // encriptamos la nueva
            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            user.PasswordHash = newPasswordHash;

            user.UpdatedAt = DateTime.UtcNow; 

            await _context.SaveChangesAsync();
        }
        public async Task<string> GenerateRecoveryToken(ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                // Por seguridad no decimos si el email existe o no
                throw new Exception("Si el correo existe, se enviarán las instrucciones.");
            }

            // se genera un token JWT para recuperación (dura 15 min)
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("purpose", "password_reset")
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task ResetPasswordWithToken(ResetPasswordDto dto)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            try
            {
                tokenHandler.ValidateToken(dto.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"], 
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero 
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var purposeClaim = jwtToken.Claims.First(c => c.Type == "purpose").Value;
                if (purposeClaim != "password_reset")
                    throw new Exception("Token inválido.");
                
                var userId = int.Parse(jwtToken.Claims.First(c => c.Type == "nameid" || c.Type == ClaimTypes.NameIdentifier).Value);

                var user = await _context.Users.FindAsync(userId);
                if (user == null) 
                    throw new Exception("No se encontró el usuario.");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch
            {
                throw new Exception("El enlace de recuperación es inválido o ha expirado.");
            }
        }

        // SEGUIR USUARIO
        public async Task FollowUserAsync(int targetUserId, int currentUserId)
        {
            // Validar que no sea a sí mismo
            if (targetUserId == currentUserId)
                throw new Exception("No puedes seguirte a ti mismo.");

            // Validar que el usuario destino exista y esté activo
            var targetExists = await _context.Users.AnyAsync(u => u.Id == targetUserId && u.IsActive);
            if (!targetExists) throw new Exception("El usuario a seguir no existe.");

            // Verificar si ya lo sigue
            var alreadyFollowing = await _context.UserFollows
                .AnyAsync(f => f.FollowerId == currentUserId && f.FollowedId == targetUserId);

            if (alreadyFollowing) throw new Exception("Ya sigues a este usuario.");

            // Crear relación
            var follow = new UserFollow
            {
                FollowerId = currentUserId,
                FollowedId = targetUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserFollows.Add(follow);
            await _context.SaveChangesAsync();
        }

        // DEJAR DE SEGUIR
        public async Task UnfollowUserAsync(int targetUserId, int currentUserId)
        {
            var follow = await _context.UserFollows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowedId == targetUserId);

            if (follow == null) throw new Exception("No sigues a este usuario.");

            _context.UserFollows.Remove(follow);
            await _context.SaveChangesAsync();
        }

        // OBTENER SEGUIDORES (Quiénes siguen a userId)
        public async Task<List<UserSummaryDto>> GetFollowersAsync(int userId, int currentUserId)
        {
            // Validar usuario base
            if (!await _context.Users.AnyAsync(u => u.Id == userId && u.IsActive))
                throw new Exception("Usuario no encontrado.");

            // OPTIMIZACIÓN: Traer primero a quiénes sigo YO (currentUserId)
            // para poder calcular el 'IsFollowing' rápidamente.
            var myFollowingIds = await _context.UserFollows
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.FollowedId)
                .ToListAsync();

            // Buscar en UserFollows donde FollowedId == userId (Los que lo siguen a él)
            var followers = await _context.UserFollows
                .Include(f => f.Follower) // Traer datos del seguidor
                .Where(f => f.FollowedId == userId && f.Follower.IsActive)
                .Select(f => f.Follower) // Nos quedamos con el objeto Usuario
                .ToListAsync();

            // Mapear a DTO
            return followers.Select(u => new UserSummaryDto
            {
                Id = u.Id,
                Username = u.Username,
                AvatarUrl = u.AvatarUrl,
                Bio = u.Bio,
                // ¿El usuario de la lista está en MI lista de seguidos?
                IsFollowing = myFollowingIds.Contains(u.Id)
            }).ToList();
        }

        // OBTENER SEGUIDOS (A quién sigue userId)
        public async Task<List<UserSummaryDto>> GetFollowingAsync(int userId, int currentUserId)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == userId && u.IsActive))
                throw new Exception("Usuario no encontrado.");

            var myFollowingIds = await _context.UserFollows
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.FollowedId)
                .ToListAsync();

            // Buscar en UserFollows donde FollowerId == userId (A quiénes sigue él)
            var following = await _context.UserFollows
                .Include(f => f.Followed) // Traer datos del seguido
                .Where(f => f.FollowerId == userId && f.Followed.IsActive)
                .Select(f => f.Followed) // Nos quedamos con el objeto Usuario
                .ToListAsync();

            return following.Select(u => new UserSummaryDto
            {
                Id = u.Id,
                Username = u.Username,
                AvatarUrl = u.AvatarUrl,
                Bio = u.Bio,
                IsFollowing = myFollowingIds.Contains(u.Id)
            }).ToList();
        }
    }
}
