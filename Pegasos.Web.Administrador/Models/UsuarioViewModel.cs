using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Pegasos.Web.Administrador.Models
{
    public class AuthResult
    {
        [JsonPropertyName("usuarioId")]
        public int UsuarioId { get; set; }

        [JsonPropertyName("nombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public string ExpiresIn { get; set; } = string.Empty;
    }
    public class UsuarioViewModel
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class CrearUsuarioViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = "Cédula";
        public string Identificacion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Rol { get; set; } = "Usuario";
        public string Password { get; set; } = string.Empty;
    }
}