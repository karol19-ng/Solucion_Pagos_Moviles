using System.ComponentModel.DataAnnotations;

namespace Pegasos.WEB.Portal.Models.ViewModels
{
    public class InscripcionViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es obligatorio")]
        [Display(Name = "Número de Cuenta")]
        public string NumeroCuenta { get; set; } = string.Empty;

        [Required(ErrorMessage = "La identificación es obligatoria")]
        [Display(Name = "Identificación")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Display(Name = "Teléfono")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
        public string Telefono { get; set; } = string.Empty;

        public string? NombreCompleto { get; set; } // Se precarga del core
    }

    public class InscripcionResult
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}