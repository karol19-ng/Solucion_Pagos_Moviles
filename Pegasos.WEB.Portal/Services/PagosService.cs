using System.Text;
using System.Text.Json;
using Pegasos.WEB.Portal.Models.InputModels;
using Pegasos.WEB.Portal.Models.ViewModels;

namespace Pegasos.WEB.Portal.Services
{
    public class PagosService : IPagosService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PagosService> _logger;

        public PagosService(HttpClient httpClient, ILogger<PagosService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<InscripcionResult?> InscribirAsync(InscribirInput input, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(input);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("gateway/auth/register", content);

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<InscripcionResult>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en inscripción");
                return new InscripcionResult { Codigo = -1, Descripcion = "Error de conexión" };
            }
        }

        public async Task<InscripcionResult?> DesinscribirAsync(DesinscribirInput input, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(input);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("gateway/auth/cancel-subscription", content);

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<InscripcionResult>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en desinscripción");
                return new InscripcionResult { Codigo = -1, Descripcion = "Error de conexión" };
            }
        }

        public async Task<TransferenciaResult?> RealizarTransferenciaAsync(TransferirInput input, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonSerializer.Serialize(input);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("gateway/transactions/route", content);

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TransferenciaResult>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en transferencia");
                return new TransferenciaResult { Codigo = -1, Descripcion = "Error de conexión" };
            }
        }
    }
}