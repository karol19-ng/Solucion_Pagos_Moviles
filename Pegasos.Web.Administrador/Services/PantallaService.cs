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

                var apiUrl = $"https://localhost:7258/api/screen/{id}";
                _logger.LogInformation("URL: {Url}", apiUrl);

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var pantalla = JsonSerializer.Deserialize<PantallaViewModel>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return pantalla;
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

                _logger.LogInformation("=== INTENTANDO CREAR PANTALLA ===");
                _logger.LogInformation("JSON enviado: {Json}", json);
                _logger.LogInformation("Headers Authorization: {Auth}", _httpClient.DefaultRequestHeaders.Authorization?.ToString());

                var apiUrl = "https://localhost:7258/api/screen";
                _logger.LogInformation("URL: {Url}", apiUrl);

                _logger.LogInformation("Enviando petición...");
                var response = await _httpClient.PostAsync(apiUrl, content);

                _logger.LogInformation("=== RESPUESTA RECIBIDA ===");
                _logger.LogInformation("StatusCode: {StatusCode} ({(int)response.StatusCode})", response.StatusCode, response.StatusCode);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Contenido: {ResponseContent}", responseContent);

                // Mostrar todos los headers de la respuesta
                _logger.LogInformation("Headers de respuesta:");
                foreach (var header in response.Headers)
                {
                    _logger.LogInformation("  {Key}: {Value}", header.Key, string.Join(", ", header.Value));
                }

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ ÉXITO - Pantalla creada");

                    // Intentar deserializar la respuesta para ver si obtenemos el ID
                    try
                    {
                        var pantallaCreada = JsonSerializer.Deserialize<PantallaViewModel>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        _logger.LogInformation("ID de pantalla creada: {Id}", pantallaCreada?.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("No se pudo deserializar respuesta: {Message}", ex.Message);
                    }

                    return true;
                }
                else
                {
                    _logger.LogWarning("❌ ERROR - Código: {StatusCode}", response.StatusCode);
                    _logger.LogWarning("Detalle: {ResponseContent}", responseContent);

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        _logger.LogWarning("Error 400 Bad Request - Posiblemente datos inválidos");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logger.LogWarning("Error 401 Unauthorized - Token inválido o expirado");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("Error 404 Not Found - URL incorrecta: {Url}", apiUrl);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        _logger.LogWarning("Error 500 Internal Server Error - Error en el servidor");
                    }

                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ HttpRequestException al crear pantalla");
                _logger.LogError("Mensaje: {Message}", ex.Message);
                _logger.LogError("Status Code: {StatusCode}", ex.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error general al crear pantalla");
                _logger.LogError("Mensaje: {Message}", ex.Message);
                _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);
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

                var apiUrl = $"https://localhost:7258/api/screen/{id}";  
                _logger.LogInformation("Eliminando pantalla {Id} desde URL: {Url}", id, apiUrl);

                var response = await _httpClient.DeleteAsync(apiUrl);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta: {StatusCode} - {Response}", response.StatusCode, responseContent);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar pantalla {id}");
                return false;
            }
        }
    }
}