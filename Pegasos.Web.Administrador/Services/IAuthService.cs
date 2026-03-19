using System;
using System.Text.Json.Serialization;

namespace Pegasos.Web.Administrador.Services
{
    public interface IAuthService
    {
        Task<AuthResult?> LoginAsync(string username, string password);
        Task<bool> ExtendSessionAsync(string accessToken);
    }

    public class AuthResult
    {
        // Solamente las propiedades que vienen del JSON (con los nombres exactos)
        [JsonPropertyName("access_token")]
        public string access_token { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string refresh_token { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public string expires_in { get; set; } = string.Empty;

        [JsonPropertyName("usuarioID")]
        public int usuarioID { get; set; }

        [JsonPropertyName("NombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        // Métodos de conveniencia (no propiedades serializables)
        public string GetAccessToken() => access_token;
        public string GetRefreshToken() => refresh_token;
        public int GetUsuarioId() => usuarioID;
        public DateTime GetExpiration()
        {
            if (DateTime.TryParse(expires_in, out var result))
                return result;
            return DateTime.UtcNow.AddMinutes(5);
        }
    }
}