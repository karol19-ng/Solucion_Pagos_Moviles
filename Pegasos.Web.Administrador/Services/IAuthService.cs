using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
namespace Pegasos.Web.Administrador.Services
{
    public interface IAuthService
    {
        Task<AuthResult?> LoginAsync(string username, string password);
        Task<bool> ExtendSessionAsync(string accessToken);
    }

    public class AuthResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresIn { get; set; }
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
    }
}
