using System.ComponentModel.DataAnnotations;

namespace Pegasos.WEB.Portal.Models.ViewModels
{
    public class SaldoViewModel
    {
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
        [Display(Name = "Número de Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        // Propiedades para la respuesta
        public decimal Saldo { get; set; }
        public string? NumeroCuenta { get; set; }
        public string? NombreCompleto { get; set; }
        public DateTime FechaConsulta { get; set; }
        public string? Identificacion { get; set; }
    }

    public class SaldoResponse
    {
        public decimal Saldo { get; set; }
        public string? NumeroCuenta { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Identificacion { get; set; }
        public int Codigo { get; set; }
        public string? Descripcion { get; set; }
    }
}