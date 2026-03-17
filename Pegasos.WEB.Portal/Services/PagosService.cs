using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
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
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonSerializer.Serialize(input);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando inscripción con token: {TokenPreview}",
                    token?.Substring(0, Math.Min(20, token?.Length ?? 0)) + "...");
                _logger.LogInformation("URL: auth/register");
                _logger.LogInformation("JSON: {Json}", json);

                var response = await _httpClient.PostAsync("auth/register", content);

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta inscripción: Status={status}, Body={body}",
                    response.StatusCode, responseJson);

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<InscripcionResult>(responseJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    _logger.LogWarning("Error en inscripción: {StatusCode} - {Response}",
                        response.StatusCode, responseJson);
                    return new InscripcionResult
                    {
                        Codigo = -1,
                        Descripcion = $"Error {response.StatusCode}: {responseJson}"
                    };
                }
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
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonSerializer.Serialize(input);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando desinscripción: {Json}", json);

                var response = await _httpClient.PostAsync("auth/cancel-subscription", content);

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta desinscripción: Status={status}, Body={body}",
                    response.StatusCode, responseJson);

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<InscripcionResult>(responseJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    return new InscripcionResult
                    {
                        Codigo = -1,
                        Descripcion = $"Error {response.StatusCode}"
                    };
                }
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
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonSerializer.Serialize(input);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando transferencia: {Json}", json);

                var response = await _httpClient.PostAsync("transactions/route", content);

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta transferencia: Status={status}, Body={body}",
                    response.StatusCode, responseJson);

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<TransferenciaResult>(responseJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    return new TransferenciaResult
                    {
                        Codigo = -1,
                        Descripcion = $"Error {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en transferencia");
                return new TransferenciaResult { Codigo = -1, Descripcion = "Error de conexión" };
            }
        }
    }
}