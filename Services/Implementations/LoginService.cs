// Services/Implementations/LoginService.cs
using AbstractDataAccess.Models;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.Implementations
{
    public class LoginService : ILoginService
    {
        private readonly PagosMovilesDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IBitacoraService _bitacoraService;

        public LoginService(PagosMovilesDbContext context, IConfiguration configuration, IBitacoraService bitacoraService)
        {
            _context = context;
            _configuration = configuration;
            _bitacoraService = bitacoraService;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.usuario) || string.IsNullOrWhiteSpace(request.password))
                throw new ArgumentException("ERROR EN LOGIN SERVICE:CAMPOS VACIOS");

            // Buscar por Nombre_Usuario (campo cambiado)
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == request.usuario);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.password, usuario.Contraseña))
                throw new UnauthorizedAccessException("Usuario o contraseña incorrectos");

            var token = GenerateJwtToken(usuario);
            var refreshToken = GenerateRefreshToken();

            var tokenExp = DateTime.Now.AddMinutes(30);
            var refreshExp = DateTime.Now.AddDays(7);

            var sesion = new InicioSesion
            {
                ID_Usuario = usuario.ID_Usuario,
                JWT_Token = token,
                Refresh_Token = refreshToken,
                Fecha_Inico = DateTime.Now,
                Fecha_Expiracion_Token = tokenExp,
                Fecha_Expiracion_Refresh = refreshExp,
                ID_Estado = 1
            };

            _context.InicioSesiones.Add(sesion);
            await _context.SaveChangesAsync();



            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuario.NombreUsuario,  
                Accion = "LOGIN",
                Descripcion = $"Usuario {usuario.NombreUsuario} inició sesión",
                Servicio = "/auth/login",
                Resultado = "OK"
            });

            return new LoginResponse
            {
                access_token = token,
                refresh_token = refreshToken,
                expires_in = tokenExp
            };
        }

        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            var sesion = await _context.InicioSesiones
                .Include(s => s.UsuarioNavigation)  // <-- CAMBIADO: de Usuario a UsuarioNavigation
                .FirstOrDefaultAsync(s => s.Refresh_Token == refreshToken && s.ID_Estado == 1);

            if (sesion == null || sesion.Fecha_Expiracion_Refresh < DateTime.Now)
                throw new UnauthorizedAccessException("No autorizado");

            var newToken = GenerateJwtToken(sesion.UsuarioNavigation);  // <-- CAMBIADO
            var newRefreshToken = GenerateRefreshToken();
            var newTokenExp = DateTime.Now.AddMinutes(30);
            var newRefreshExp = DateTime.Now.AddDays(7);

            sesion.JWT_Token = newToken;
            sesion.Refresh_Token = newRefreshToken;
            sesion.Fecha_Expiracion_Token = newTokenExp;
            sesion.Fecha_Expiracion_Refresh = newRefreshExp;

            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                access_token = newToken,
                refresh_token = newRefreshToken,
                expires_in = newTokenExp
            };
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var sesion = await _context.InicioSesiones
                .FirstOrDefaultAsync(s => s.JWT_Token == token && s.ID_Estado == 1);

            if (sesion == null || sesion.Fecha_Expiracion_Token < DateTime.Now)
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync(string token)
        {
            var sesion = await _context.InicioSesiones
                .FirstOrDefaultAsync(s => s.JWT_Token == token);

            if (sesion != null)
            {
                sesion.ID_Estado = 2;
                await _context.SaveChangesAsync();
            }
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.ID_Usuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),  
                    new Claim(ClaimTypes.Email, usuario.Email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}