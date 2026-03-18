using System.Text.Json.Serialization;

namespace Pegasos.WEB.Portal.Models.InputModels
{
    public class DesinscribirInput
    {
        [JsonPropertyName("Numero_Cuenta")]
        public string NumeroCuenta { get; set; } = string.Empty;

        [JsonPropertyName("Identificacion")]
        public string Identificacion { get; set; } = string.Empty;

        [JsonPropertyName("Numero_Telefono")]
        public string Telefono { get; set; } = string.Empty;
    }
}