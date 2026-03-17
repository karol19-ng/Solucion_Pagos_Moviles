using System.ComponentModel.DataAnnotations;

namespace Pegasos.WEB.Portal.Models.ViewModels
{
    public class InscripcionViewModel
    {
        [Display(Name = "Número de Cuenta")]
        public string NumeroCuenta { get; set; } = string.Empty;

        [Display(Name = "Identificación")]
        public string Identificacion { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
        public string Telefono { get; set; } = string.Empty;

        public string? NombreCompleto { get; set; }
    }

    public class InscripcionResult
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}