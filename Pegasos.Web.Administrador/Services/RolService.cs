using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public class RolService : IRolService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RolService> _logger;

        public RolService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<RolService> logger)
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

        public async Task<List<RolViewModel>?> ListarTodosAsync()
        {
            try
            {
                AgregarTokenAlHeader();

                var apiUrl = "https://localhost:7258/rol";
                _logger.LogInformation("Listando roles desde: {Url}", apiUrl);

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Respuesta JSON: {Json}", json);

                    using JsonDocument doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var roles = new List<RolViewModel>();

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in root.EnumerateArray())
                        {
                            var rol = new RolViewModel
                            {
                                Id = item.TryGetProperty("iD_Rol", out var idProp) ? idProp.GetInt32() :
                                     item.TryGetProperty("ID_Rol", out idProp) ? idProp.GetInt32() : 0,
                                Nombre = item.TryGetProperty("nombre", out var nombreProp) ? nombreProp.GetString() ?? "" :
                                         item.TryGetProperty("Nombre", out nombreProp) ? nombreProp.GetString() ?? "" : "",
                                // 🆕 MAPEAR DESCRIPCION
                                Descripcion = item.TryGetProperty("descripcion", out var descProp) ? descProp.GetString() ?? "" :
                                              item.TryGetProperty("Descripcion", out descProp) ? descProp.GetString() ?? "" : ""
                            };

                            // Mapear pantallas si existen
                            if (item.TryGetProperty("pantallas", out var pantallasProp) ||
                                item.TryGetProperty("Pantallas", out pantallasProp))
                            {
                                if (pantallasProp.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var p in pantallasProp.EnumerateArray())
                                    {
                                        var pantalla = new PantallaAsignadaViewModel
                                        {
                                            Id = p.TryGetProperty("iD_Pantalla", out var pid) ? pid.GetInt32() :
                                                 p.TryGetProperty("ID_Pantalla", out pid) ? pid.GetInt32() : 0,
                                            Nombre = p.TryGetProperty("nombre", out var pnombre) ? pnombre.GetString() ?? "" :
                                                     p.TryGetProperty("Nombre", out pnombre) ? pnombre.GetString() ?? "" : "",
                                            Descripcion = p.TryGetProperty("descripcion", out var pdesc) ? pdesc.GetString() ?? "" :
                                                          p.TryGetProperty("Descripcion", out pdesc) ? pdesc.GetString() ?? "" : "",
                                            Asignada = true
                                        };
                                        rol.Pantallas.Add(pantalla);
                                    }
                                }
                            }

                            roles.Add(rol);
                        }
                    }

                    _logger.LogInformation("Roles mapeados: {Count}", roles.Count);
                    foreach (var rol in roles)
                    {
                        _logger.LogInformation("Rol mapeado - ID: {Id}, Nombre: {Nombre}, Descripción: {Descripcion}",
                            rol.Id, rol.Nombre, rol.Descripcion);
                    }

                    return roles;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al listar roles: {StatusCode} - {Error}", response.StatusCode, error);
                    return new List<RolViewModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar roles");
                return new List<RolViewModel>();
            }
        }

        public async Task<RolViewModel?> ObtenerPorIdAsync(int id)
        {
            try
            {
                AgregarTokenAlHeader();

                var apiUrl = $"https://localhost:7258/rol/{id}";
                _logger.LogInformation("Obteniendo rol {Id} desde: {Url}", id, apiUrl);

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Respuesta JSON: {Json}", json);

                    using JsonDocument doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var rol = new RolViewModel();

                    // Mapear ID_Rol
                    if (root.TryGetProperty("iD_Rol", out var idProp))
                        rol.Id = idProp.GetInt32();
                    else if (root.TryGetProperty("ID_Rol", out idProp))
                        rol.Id = idProp.GetInt32();

                    // Mapear Nombre
                    if (root.TryGetProperty("nombre", out var nombreProp))
                        rol.Nombre = nombreProp.GetString() ?? "";
                    else if (root.TryGetProperty("Nombre", out nombreProp))
                        rol.Nombre = nombreProp.GetString() ?? "";

                    // ✅ MAPEAR DESCRIPCION
                    if (root.TryGetProperty("descripcion", out var descProp))
                        rol.Descripcion = descProp.GetString() ?? "";
                    else if (root.TryGetProperty("Descripcion", out descProp))
                        rol.Descripcion = descProp.GetString() ?? "";

                    // Mapear Pantallas
                    if (root.TryGetProperty("pantallas", out var pantallasProp) ||
                        root.TryGetProperty("Pantallas", out pantallasProp))
                    {
                        if (pantallasProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in pantallasProp.EnumerateArray())
                            {
                                var pantalla = new PantallaAsignadaViewModel();

                                if (item.TryGetProperty("iD_Pantalla", out var pid))
                                    pantalla.Id = pid.GetInt32();
                                else if (item.TryGetProperty("ID_Pantalla", out pid))
                                    pantalla.Id = pid.GetInt32();

                                if (item.TryGetProperty("nombre", out var pnombre))
                                    pantalla.Nombre = pnombre.GetString() ?? "";
                                else if (item.TryGetProperty("Nombre", out pnombre))
                                    pantalla.Nombre = pnombre.GetString() ?? "";

                                pantalla.Asignada = true;
                                rol.Pantallas.Add(pantalla);
                            }
                        }
                    }

                    _logger.LogInformation("Rol mapeado - ID: {Id}, Nombre: {Nombre}, Descripción: {Descripcion}, Pantallas: {Count}",
                        rol.Id, rol.Nombre, rol.Descripcion, rol.Pantallas.Count);

                    return rol;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al obtener rol: {StatusCode} - {Error}", response.StatusCode, error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener rol {id}");
                return null;
            }
        }

        public async Task<List<PantallaAsignadaViewModel>?> ObtenerPantallasConAsignacionAsync(int rolId = 0)
        {
            try
            {
                AgregarTokenAlHeader();

                var pantallasUrl = "https://localhost:7258/api/screen";
                _logger.LogInformation("Obteniendo pantallas desde: {Url}", pantallasUrl);

                var response = await _httpClient.GetAsync(pantallasUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Respuesta pantallas: {Json}", json);

                    // Usar JsonDocument para mapear manualmente
                    using JsonDocument doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var pantallas = new List<PantallaAsignadaViewModel>();

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in root.EnumerateArray())
                        {
                            int id = 0;
                            if (item.TryGetProperty("iD_Pantalla", out var idProp))
                                id = idProp.GetInt32();
                            else if (item.TryGetProperty("ID_Pantalla", out idProp))
                                id = idProp.GetInt32();

                            string nombre = "";
                            if (item.TryGetProperty("nombre", out var nombreProp))
                                nombre = nombreProp.GetString() ?? "";
                            else if (item.TryGetProperty("Nombre", out nombreProp))
                                nombre = nombreProp.GetString() ?? "";

                            string descripcion = "";
                            if (item.TryGetProperty("descripcion", out var descProp))
                                descripcion = descProp.GetString() ?? "";
                            else if (item.TryGetProperty("Descripcion", out descProp))
                                descripcion = descProp.GetString() ?? "";

                            pantallas.Add(new PantallaAsignadaViewModel
                            {
                                Id = id,
                                Nombre = nombre,
                                Descripcion = descripcion,
                                Asignada = false
                            });
                        }
                    }

                    _logger.LogInformation("Pantallas mapeadas: {Count}", pantallas.Count);

                    // Si hay un rolId, obtener sus pantallas asignadas
                    if (rolId > 0)
                    {
                        var rol = await ObtenerPorIdAsync(rolId);
                        if (rol != null && rol.Pantallas != null)
                        {
                            var pantallasAsignadas = rol.Pantallas.Select(p => p.Id).ToList();
                            _logger.LogInformation("Pantallas asignadas al rol {RolId}: {Pantallas}", rolId, string.Join(",", pantallasAsignadas));

                            foreach (var pantalla in pantallas)
                            {
                                pantalla.Asignada = pantallasAsignadas.Contains(pantalla.Id);
                            }
                        }
                    }

                    return pantallas;
                }

                return new List<PantallaAsignadaViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pantallas con asignación");
                return new List<PantallaAsignadaViewModel>();
            }
        }

        public async Task<bool> CrearAsync(CrearRolViewModel model)
        {
            try
            {
                AgregarTokenAlHeader();

                // Crear el objeto que espera la API - usar los nombres exactos que espera el DTO
                var request = new
                {
                    id_Rol = 0,  // La API generará el ID
                    nombre = model.Nombre,
                    descripcion = model.Descripcion ?? "",
                    pantallas = model.PantallasSeleccionadas ?? new List<int>()
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("=== CREANDO ROL ===");
                _logger.LogInformation("JSON enviado: {Json}", json);
                _logger.LogInformation("Headers Authorization: {Auth}", _httpClient.DefaultRequestHeaders.Authorization?.ToString());

                var apiUrl = "https://localhost:7258/rol";
                _logger.LogInformation("URL: {Url}", apiUrl);

                var response = await _httpClient.PostAsync(apiUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("StatusCode: {StatusCode} ({(int)response.StatusCode})", response.StatusCode, response.StatusCode);
                _logger.LogInformation("Respuesta del servidor: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Rol creado exitosamente");
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning("❌ Bad Request - Error de validación");
                    _logger.LogWarning("Detalle: {Response}", responseContent);
                    return false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("❌ No autorizado - Token inválido");
                    return false;
                }
                else
                {
                    _logger.LogWarning("❌ Error inesperado: {StatusCode}", response.StatusCode);
                    _logger.LogWarning("Detalle: {Response}", responseContent);
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ HttpRequestException al crear rol");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error general al crear rol");
                return false;
            }
        }

        public async Task<bool> ActualizarAsync(EditarRolViewModel model)
        {
            try
            {
                AgregarTokenAlHeader();

                var pantallasValidas = model.PantallasSeleccionadas?.Where(id => id > 0).ToList() ?? new List<int>();

                var request = new
                {
                    id_Rol = model.Id,
                    nombre = model.Nombre,
                    descripcion = model.Descripcion ?? "",
                    pantallas = pantallasValidas
                    // ❌ ELIMINAR descripcionesPorPantalla
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("=== ACTUALIZANDO ROL ===");
                _logger.LogInformation("JSON enviado: {Json}", json);

                var apiUrl = $"https://localhost:7258/rol/{model.Id}";
                var response = await _httpClient.PutAsync(apiUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("StatusCode: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseContent);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol {Id}", model.Id);
                return false;
            }
        }

        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                AgregarTokenAlHeader();

                var apiUrl = $"https://localhost:7258/rol/{id}";
                _logger.LogInformation("=== ELIMINANDO ROL ===");
                _logger.LogInformation("URL: {Url}", apiUrl);
                _logger.LogInformation("ID: {Id}", id);

                var response = await _httpClient.DeleteAsync(apiUrl);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("StatusCode: {StatusCode} ({(int)response.StatusCode})", response.StatusCode, response.StatusCode);
                _logger.LogInformation("Respuesta completa: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Rol eliminado exitosamente");

                    // Intentar parsear la respuesta
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(responseContent);
                        _logger.LogInformation("Respuesta parseada correctamente");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("No se pudo parsear JSON: {Message}", ex.Message);
                    }

                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("❌ Rol no encontrado (404)");
                    return false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning("❌ Bad Request (400) - Posiblemente tiene dependencias");
                    _logger.LogWarning("Detalle: {Response}", responseContent);
                    return false;
                }
                else
                {
                    _logger.LogWarning("❌ Error inesperado: {StatusCode}", response.StatusCode);
                    _logger.LogWarning("Detalle: {Response}", responseContent);
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ HttpRequestException al eliminar rol {Id}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error general al eliminar rol {Id}", id);
                return false;
            }
        }
    }
}