using System.ComponentModel.DataAnnotations;

namespace Pegasos.WEB.Portal.Models.ViewModels
{
    public class LoginPortalViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Display(Name = "Usuario")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class AuthResult
    {
        // IMPORTANTE: Estos nombres deben coincidir con la respuesta del microservicio
        public string access_token { get; set; } = string.Empty;
        public string refresh_token { get; set; } = string.Empty;
        public DateTime expires_in { get; set; }

        // El microservicio devuelve SOLO access_token, refresh_token y expires_in
        // Los otros datos (usuarioId, nombreCompleto) los obtendremos del token JWT
        // o de una consulta adicional
    }
}