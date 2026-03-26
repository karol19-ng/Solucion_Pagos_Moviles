using System.Text.Json;
using System.Net.Http.Headers;
using Pegasos.WEB.Portal.Models.ViewModels;

namespace Pegasos.WEB.Portal.Services
{
    public class ConsultasService : IConsultasService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ConsultasService> _logger;

        public ConsultasService(HttpClient httpClient, ILogger<ConsultasService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SaldoViewModel?> ConsultarSaldoAsync(string telefono, string identificacion, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var url = $"gateway/accounts/balance?telefono={telefono}&identificacion={identificacion}";

                _logger.LogInformation("Consultando saldo: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Status Code: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseJson);

                if (response.IsSuccessStatusCode)
                {
                    // Deserializar correctamente a un objeto específico
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<SaldoResponseDto>(responseJson, options);

                    if (result != null)
                    {
                        return new SaldoViewModel
                        {
                            Telefono = telefono,
                            Identificacion = identificacion,
                            Saldo = result.saldo ?? 0,
                            NumeroCuenta = result.numeroCuenta ?? result.Numero_Cuenta ?? "",
                            NombreCompleto = result.nombreCompleto ?? result.Nombre_Completo ?? "",
                            FechaConsulta = DateTime.Now
                        };
                    }
                }

                // Manejo de errores según código HTTP
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Cliente no encontrado: {Identificacion}", identificacion);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning("Datos inválidos: {Response}", responseJson);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    _logger.LogError("Error interno del servidor: {Response}", responseJson);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando saldo");
                return null;
            }
        }

        public async Task<MovimientosViewModel?> ConsultarMovimientosAsync(string telefono, string identificacion, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var url = $"gateway/accounts/transactions?telefono={telefono}&identificacion={identificacion}";

                var response = await _httpClient.GetAsync(url);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<MovimientosViewModel>(responseJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando movimientos");
                return null;
            }
        }
    }

    // DTO para la respuesta del saldo
    public class SaldoResponseDto
    {
        public decimal? saldo { get; set; }
        public string? numeroCuenta { get; set; }
        public string? Numero_Cuenta { get; set; }
        public string? nombreCompleto { get; set; }
        public string? Nombre_Completo { get; set; }
        public int? codigo { get; set; }
        public string? descripcion { get; set; }
    }
}