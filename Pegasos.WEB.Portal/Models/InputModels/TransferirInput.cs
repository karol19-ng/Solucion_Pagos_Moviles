using System.ComponentModel.DataAnnotations;

namespace Pegasos.WEB.Portal.Models.InputModels
{
    public class TransferirInput
    {
        [Required(ErrorMessage = "El teléfono origen es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
        public string TelefonoOrigen { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono destino es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
        public string TelefonoDestino { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(1, 100000, ErrorMessage = "El monto debe estar entre 1 y 100,000")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(25, ErrorMessage = "La descripción no puede superar 25 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        public string? NombreOrigen { get; set; } // Se obtiene del core
    }

    public class TransferenciaResult
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string? Comprobante { get; set; }
    }
}