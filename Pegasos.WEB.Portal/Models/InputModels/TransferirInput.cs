using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pegasos.WEB.Portal.Models.InputModels
{
    public class TransferirInput
    {
        [Required(ErrorMessage = "El teléfono origen es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
        [JsonPropertyName("Telefono_Origen")]
        public string TelefonoOrigen { get; set; } = string.Empty;

        [JsonPropertyName("Nombre_Origen")]
        public string NombreOrigen { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono destino es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
        [JsonPropertyName("Telefono_Destino")]
        public string TelefonoDestino { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(1, 100000, ErrorMessage = "El monto debe estar entre 1 y 100,000")]
        [JsonPropertyName("Monto")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(25, ErrorMessage = "La descripción no puede superar 25 caracteres")]
        [JsonPropertyName("Descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }
}