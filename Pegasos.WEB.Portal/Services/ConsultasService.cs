using System.Text;
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

                _logger.LogInformation("=== CONSULTA SALDO ===");
                _logger.LogInformation("Teléfono: {Telefono}", telefono);
                _logger.LogInformation("Identificación: {Identificacion}", identificacion);

                // Construir URL con los parámetros requeridos
                var url = $"gateway/accounts/balance?telefono={telefono}&identificacion={identificacion}";
                _logger.LogInformation("URL: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Status Code: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseJson);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<SaldoResponse>(responseJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result != null)
                    {
                        return new SaldoViewModel
                        {
                            Telefono = telefono,
                            Identificacion = identificacion,
                            Saldo = result.Saldo,
                            NumeroCuenta = result.NumeroCuenta,
                            NombreCompleto = result.NombreCompleto,
                            FechaConsulta = DateTime.Now
                        };
                    }
                }
                else
                {
                    _logger.LogWarning("Error en consulta de saldo: {StatusCode} - {Response}",
                        response.StatusCode, responseJson);
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

                _logger.LogInformation("=== CONSULTA MOVIMIENTOS ===");
                _logger.LogInformation("Teléfono: {Telefono}", telefono);
                _logger.LogInformation("Identificación: {Identificacion}", identificacion);

                var url = $"gateway/accounts/transactions?telefono={telefono}&identificacion={identificacion}";
                _logger.LogInformation("URL: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Status Code: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseJson);

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
}