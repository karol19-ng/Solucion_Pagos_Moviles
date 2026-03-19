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

                _logger.LogInformation("=== LISTANDO PANTALLAS ===");
                _logger.LogInformation("BaseAddress del HttpClient: {BaseAddress}", _httpClient.BaseAddress);

                // Usar URL directa a la API (como en Roles)
                var apiUrl = "https://localhost:7258/api/screen";
                _logger.LogInformation("URL: {Url}", apiUrl);

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Respuesta JSON: {Json}", json);

                    // Usar JsonDocument para mapear manualmente
                    using JsonDocument doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var pantallas = new List<PantallaViewModel>();

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in root.EnumerateArray())
                        {
                            var pantalla = new PantallaViewModel();

                            // Mapear ID (puede venir como iD_Pantalla, ID_Pantalla, etc.)
                            if (item.TryGetProperty("iD_Pantalla", out var idProp))
                                pantalla.Id = idProp.GetInt32();
                            else if (item.TryGetProperty("ID_Pantalla", out idProp))
                                pantalla.Id = idProp.GetInt32();
                            else if (item.TryGetProperty("id", out idProp))
                                pantalla.Id = idProp.GetInt32();

                            // Mapear Nombre
                            if (item.TryGetProperty("nombre", out var nombreProp))
                                pantalla.Nombre = nombreProp.GetString() ?? "";
                            else if (item.TryGetProperty("Nombre", out nombreProp))
                                pantalla.Nombre = nombreProp.GetString() ?? "";

                            // Mapear Descripción
                            if (item.TryGetProperty("descripcion", out var descProp))
                                pantalla.Descripcion = descProp.GetString() ?? "";
                            else if (item.TryGetProperty("Descripcion", out descProp))
                                pantalla.Descripcion = descProp.GetString() ?? "";

                            // Mapear Ruta
                            if (item.TryGetProperty("ruta", out var rutaProp))
                                pantalla.Ruta = rutaProp.GetString() ?? "";
                            else if (item.TryGetProperty("Ruta", out rutaProp))
                                pantalla.Ruta = rutaProp.GetString() ?? "";

                            // Mapear Estado
                            if (item.TryGetProperty("estado", out var estadoProp))
                                pantalla.Estado = estadoProp.GetInt32();
                            else if (item.TryGetProperty("Estado", out estadoProp))
                                pantalla.Estado = estadoProp.GetInt32();

                            _logger.LogInformation("Pantalla mapeada - ID: {Id}, Nombre: {Nombre}", pantalla.Id, pantalla.Nombre);
                            pantallas.Add(pantalla);
                        }
                    }

                    _logger.LogInformation("Total pantallas mapeadas: {Count}", pantallas.Count);
                    return pantallas;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al listar pantallas: {StatusCode} - {Error}", response.StatusCode, error);
                    return new List<PantallaViewModel>();
                }
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
                _logger.LogInformation("Obteniendo pantalla {Id} desde: {Url}", id, apiUrl);

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Respuesta JSON: {Json}", json);

                    using JsonDocument doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var pantalla = new PantallaViewModel();

                    // Mapear ID
                    if (root.TryGetProperty("iD_Pantalla", out var idProp))
                        pantalla.Id = idProp.GetInt32();
                    else if (root.TryGetProperty("ID_Pantalla", out idProp))
                        pantalla.Id = idProp.GetInt32();

                    // Mapear Nombre
                    if (root.TryGetProperty("nombre", out var nombreProp))
                        pantalla.Nombre = nombreProp.GetString() ?? "";
                    else if (root.TryGetProperty("Nombre", out nombreProp))
                        pantalla.Nombre = nombreProp.GetString() ?? "";

                    // Mapear Descripción
                    if (root.TryGetProperty("descripcion", out var descProp))
                        pantalla.Descripcion = descProp.GetString() ?? "";
                    else if (root.TryGetProperty("Descripcion", out descProp))
                        pantalla.Descripcion = descProp.GetString() ?? "";

                    // Mapear Ruta
                    if (root.TryGetProperty("ruta", out var rutaProp))
                        pantalla.Ruta = rutaProp.GetString() ?? "";
                    else if (root.TryGetProperty("Ruta", out rutaProp))
                        pantalla.Ruta = rutaProp.GetString() ?? "";

                    // Mapear Estado
                    if (root.TryGetProperty("estado", out var estadoProp))
                        pantalla.Estado = estadoProp.GetInt32();
                    else if (root.TryGetProperty("Estado", out estadoProp))
                        pantalla.Estado = estadoProp.GetInt32();

                    _logger.LogInformation("Pantalla mapeada - ID: {Id}, Nombre: {Nombre}", pantalla.Id, pantalla.Nombre);
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

                _logger.LogInformation("=== ACTUALIZANDO PANTALLA ===");
                _logger.LogInformation("Modelo - Id: {Id}, Nombre: {Nombre}, Descripción: {Descripcion}, Ruta: {Ruta}, Estado: {Estado}",
                    model.Id, model.Nombre, model.Descripcion, model.Ruta, model.Estado);

                // Crear el objeto con TODOS los campos
                var request = new
                {
                    id_Pantalla = model.Id,
                    nombre = model.Nombre,
                    descripcion = model.Descripcion,
                    ruta = model.Ruta,
                    estado = model.Estado  // ← ESTO ES LO QUE FALTA
                };

                var json = JsonSerializer.Serialize(request);
                _logger.LogInformation("JSON enviado: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiUrl = $"https://localhost:7258/api/screen/{model.Id}";
                _logger.LogInformation("URL: {Url}", apiUrl);

                var response = await _httpClient.PutAsync(apiUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Pantalla actualizada exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("❌ Error al actualizar: {StatusCode}", response.StatusCode);
                    return false;
                }
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
                _logger.LogInformation("=== ELIMINANDO PANTALLA ===");
                _logger.LogInformation("URL: {Url}", apiUrl);

                var response = await _httpClient.DeleteAsync(apiUrl);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    if (result != null && result.ContainsKey("codigo") && result["codigo"]?.ToString() == "0")
                    {
                        return true;
                    }
                    return true; // Si es éxito aunque no tenga el formato esperado
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