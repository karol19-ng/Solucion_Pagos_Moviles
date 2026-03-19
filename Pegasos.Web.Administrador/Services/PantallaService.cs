using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public class PantallaService : IPantallaService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PantallaService> _logger;

        public PantallaService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PantallaService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private string ObtenerToken()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user?.Identity?.IsAuthenticated != true)
                {
                    _logger.LogWarning("Usuario no autenticado al intentar obtener token");
                    throw new UnauthorizedAccessException("Usuario no autenticado");
                }

                var token = user.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;

                if (string.IsNullOrEmpty(token))
                {
                    var claimsList = string.Join(", ", user.Claims.Select(c => $"{c.Type}"));
                    _logger.LogError("No se encontró claim 'access_token'. Claims disponibles: {Claims}", claimsList);
                    throw new UnauthorizedAccessException("No hay token de autenticación disponible en los claims");
                }

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener token de autenticación");
                throw;
            }
        }

        private void AgregarTokenAlHeader()
        {
            var token = ObtenerToken();

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Remove("Authorization");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<PantallaViewModel>?> ListarTodosAsync()
        {
            try
            {
                AgregarTokenAlHeader();

                _logger.LogInformation("=== DIAGNÓSTICO DE PANTALLAS ===");
                _logger.LogInformation("BaseAddress del HttpClient: {BaseAddress}", _httpClient.BaseAddress);

                // Mostrar todos los headers
                foreach (var header in _httpClient.DefaultRequestHeaders)
                {
                    _logger.LogInformation("Header: {Key} = {Value}", header.Key, string.Join(",", header.Value));
                }

                // Probar diferentes URLs
                var urlsToTest = new[]
                {
            "gateway/api/screen",
            "api/screen",
            "screen",
            "https://localhost:7258/screen",
            "https://localhost:7258/api/screen"
        };

                foreach (var testUrl in urlsToTest)
                {
                    _logger.LogInformation("Probando URL: {Url}", testUrl);
                    try
                    {
                        var response = await _httpClient.GetAsync(testUrl);
                        _logger.LogInformation("URL {Url} - StatusCode: {StatusCode}", testUrl, response.StatusCode);

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            _logger.LogInformation("✅ ÉXITO con URL {Url}: {Json}", testUrl, json);

                            var pantallas = JsonSerializer.Deserialize<List<PantallaViewModel>>(json, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            return pantallas;
                        }
                        else
                        {
                            var error = await response.Content.ReadAsStringAsync();
                            _logger.LogWarning("❌ Falló URL {Url}: {StatusCode} - {Error}", testUrl, response.StatusCode, error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Excepción con URL {Url}", testUrl);
                    }
                }

                return new List<PantallaViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar pantallas");
                return new List<PantallaViewModel>();
            }
        }

        public async Task<PantallaViewModel?> ObtenerPorIdAsync(int id)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/Screen/{id}";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PantallaResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Pantalla;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener pantalla {id}");
                return null;
            }
        }

        public async Task<bool> CrearAsync(CrearPantallaViewModel model)
        {
            try
            {
                AgregarTokenAlHeader();

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Creando pantalla: {Json}", json);

                var response = await _httpClient.PostAsync("gateway/api/Screen", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta: {StatusCode} - {Response}", response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<PantallaResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Codigo == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pantalla");
                return false;
            }
        }

        public async Task<bool> ActualizarAsync(EditarPantallaViewModel model)
        {
            try
            {
                AgregarTokenAlHeader();

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"gateway/api/Screen/{model.Id}";
                _logger.LogInformation("Actualizando pantalla {Id}: {Json}", model.Id, json);

                var response = await _httpClient.PutAsync(url, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta: {StatusCode} - {Response}", response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<PantallaResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Codigo == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar pantalla {model.Id}");
                return false;
            }
        }

        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/Screen/{id}";
                _logger.LogInformation("Eliminando pantalla {Id}", id);

                var response = await _httpClient.DeleteAsync(url);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta: {StatusCode} - {Response}", response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<PantallaResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Codigo == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar pantalla {id}");
                return false;
            }
        }
    }
}