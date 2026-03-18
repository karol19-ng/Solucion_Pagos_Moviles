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

                var json = JsonSerializer.Serialize(input, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("=== INSCRIPCIÓN ===");
                _logger.LogInformation("URL: gateway/auth/register");
                _logger.LogInformation("Token enviado: Bearer {Token}",
                    !string.IsNullOrEmpty(token) ? token.Substring(0, Math.Min(20, token.Length)) + "..." : "VACÍO");
                _logger.LogInformation("JSON Enviado: {Json}", json);

                var response = await _httpClient.PostAsync("gateway/auth/register", content);

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Status Code: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseJson);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<InscripcionResult>(responseJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    _logger.LogInformation("Inscripción exitosa: Código={Codigo}, Descripción={Descripcion}",
                        result?.Codigo, result?.Descripcion);

                    return result;
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
                return new InscripcionResult
                {
                    Codigo = -1,
                    Descripcion = "Error de conexión: " + ex.Message
                };
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

                var json = JsonSerializer.Serialize(input, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("=== DESINSCRIPCIÓN ===");
                _logger.LogInformation("URL: gateway/auth/cancel-subscription");
                _logger.LogInformation("Token enviado: Bearer {Token}",
                    !string.IsNullOrEmpty(token) ? token.Substring(0, Math.Min(20, token.Length)) + "..." : "VACÍO");
                _logger.LogInformation("JSON Enviado: {Json}", json);

                var response = await _httpClient.PostAsync("gateway/auth/cancel-subscription", content);

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Status Code: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseJson);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<InscripcionResult>(responseJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    _logger.LogInformation("Desinscripción exitosa: Código={Codigo}, Descripción={Descripcion}",
                        result?.Codigo, result?.Descripcion);

                    return result;
                }
                else
                {
                    _logger.LogWarning("Error en desinscripción: {StatusCode} - {Response}",
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
                _logger.LogError(ex, "Error en desinscripción");
                return new InscripcionResult
                {
                    Codigo = -1,
                    Descripcion = "Error de conexión: " + ex.Message
                };
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

                var json = JsonSerializer.Serialize(input, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("=== TRANSFERENCIA ===");
                _logger.LogInformation("URL: gateway/transactions/route");
                _logger.LogInformation("Token enviado: Bearer {Token}",
                    !string.IsNullOrEmpty(token) ? token.Substring(0, Math.Min(20, token.Length)) + "..." : "VACÍO");
                _logger.LogInformation("JSON Enviado: {Json}", json);

                var response = await _httpClient.PostAsync("gateway/transactions/route", content);

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Status Code: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseJson);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TransferenciaResult>(responseJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    _logger.LogInformation("Transferencia exitosa: Código={Codigo}, Descripción={Descripcion}, Comprobante={Comprobante}",
                        result?.Codigo, result?.Descripcion, result?.Comprobante);

                    return result;
                }
                else
                {
                    _logger.LogWarning("Error en transferencia: {StatusCode} - {Response}",
                        response.StatusCode, responseJson);

                    return new TransferenciaResult
                    {
                        Codigo = -1,
                        Descripcion = $"Error {response.StatusCode}: {responseJson}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en transferencia");
                return new TransferenciaResult
                {
                    Codigo = -1,
                    Descripcion = "Error de conexión: " + ex.Message
                };
            }
        }
    }
}