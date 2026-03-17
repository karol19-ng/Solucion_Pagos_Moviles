using System.Text.Json;
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
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"gateway/accounts/balance?telefono={telefono}&identificacion={identificacion}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<SaldoViewModel>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
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
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"gateway/accounts/transactions?telefono={telefono}&identificacion={identificacion}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<MovimientosViewModel>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
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