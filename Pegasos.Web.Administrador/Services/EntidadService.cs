using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public class EntidadService : IEntidadService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EntidadService> _logger;

        public EntidadService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<EntidadService> logger)
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

                _logger.LogDebug("Token obtenido exitosamente. Longitud: {Length}", token.Length);
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

            _logger.LogDebug("Token agregado al header Authorization");
        }

        // Listar todas las entidades
        public async Task<List<EntidadViewModel>?> ListarTodosAsync()
        {
            try
            {
                AgregarTokenAlHeader();

                var url = "gateway/api/entidad";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Respuesta exitosa de Gateway");

                    var result = JsonSerializer.Deserialize<EntidadResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Mapear los datos de la respuesta al modelo EntidadViewModel
                    if (result?.Entidades != null)
                    {
                        foreach (var entidad in result.Entidades)
                        {
                            // Asegurar que EstadoDescripcion esté correctamente asignado
                            if (entidad.EstadoId == 1)
                                entidad.EstadoDescripcion = "Activo";
                            else if (entidad.EstadoId == 0)
                                entidad.EstadoDescripcion = "Inactivo";
                        }
                    }

                    return result?.Entidades ?? new List<EntidadViewModel>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gateway: Token no autorizado (401). Detalle: {Error}", error);
                    throw new UnauthorizedAccessException($"Token rechazado por Gateway: {error}");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error al listar entidades: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return new List<EntidadViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar entidades");
                throw;
            }
        }

        // Obtener por ID
        public async Task<EntidadViewModel?> ObtenerPorIdAsync(int id)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/entidad/{id}";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Respuesta de Gateway para ID {Id}: {Json}", id, json);

                    var result = JsonSerializer.Deserialize<EntidadResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Asegurar que EstadoDescripción esté correctamente asignado
                    if (result?.Entidad != null)
                    {
                        if (result.Entidad.EstadoId == 1)
                            result.Entidad.EstadoDescripcion = "Activo";
                        else if (result.Entidad.EstadoId == 0)
                            result.Entidad.EstadoDescripcion = "Inactivo";
                    }

                    return result?.Entidad;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Entidad con ID {Id} no encontrada", id);
                    return null;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al obtener entidad {Id}: {StatusCode} - {Error}",
                    id, response.StatusCode, errorContent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener entidad {id}");
                throw;
            }
        }

        // Crear entidad
        public async Task<(bool Exito, string Mensaje, int? EntidadId)> CrearAsync(CrearEntidadViewModel model)
        {
            try
            {
                AgregarTokenAlHeader();

                // Crear objeto con la estructura esperada por la API
                var requestData = new
                {
                    identificador = model.Identificador,
                    nombre = model.Nombre
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("=== INICIANDO CREACIÓN DE ENTIDAD ===");
                _logger.LogInformation("JSON enviado: {Json}", json);
                _logger.LogInformation("Identificador: {Identificador}, Nombre: {Nombre}",
                    model.Identificador, model.Nombre);

                var response = await _httpClient.PostAsync("gateway/api/entidad", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta del servidor: {ResponseContent}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<EntidadResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Codigo == 0)
                    {
                        _logger.LogInformation("Entidad creada exitosamente. ID: {EntidadId}, Identificador: {Identificador}",
                            result.Entidad?.Id, model.Identificador);
                        return (true, "Entidad creada exitosamente", result.Entidad?.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Error en creación: {Descripcion}", result?.Descripcion);
                        return (false, result?.Descripcion ?? "Error al crear la entidad", null);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogWarning("Conflicto: Ya existe una entidad con identificador {Identificador}", model.Identificador);
                    return (false, $"Ya existe una entidad con identificador '{model.Identificador}'", null);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning("Error de validación: {Error}", responseContent);
                    return (false, $"Error de validación: {responseContent}", null);
                }
                else
                {
                    _logger.LogWarning("Error en creación. StatusCode: {StatusCode}", response.StatusCode);
                    _logger.LogWarning("Detalle: {Error}", responseContent);
                    return (false, $"Error del servidor: {response.StatusCode} - {responseContent}", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear entidad");
                return (false, $"Error al crear entidad: {ex.Message}", null);
            }
        }

        // Actualizar entidad
        public async Task<bool> ActualizarAsync(EditarEntidadViewModel model)
        {
            try
            {
                AgregarTokenAlHeader();

                // Crear objeto con la estructura esperada por la API
                var requestData = new
                {
                    identificador = model.Identificador,
                    nombre = model.Nombre,
                    estadoId = model.EstadoId
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"gateway/api/entidad/{model.Id}";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);
                _logger.LogInformation("JSON enviado: {Json}", json);

                var response = await _httpClient.PutAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta del servidor: {ResponseContent}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<EntidadResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var exito = result?.Codigo == 0;
                    if (exito)
                    {
                        _logger.LogInformation("Entidad {Id} actualizada exitosamente", model.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Error al actualizar entidad {Id}: {Descripcion}", model.Id, result?.Descripcion);
                    }

                    return exito;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogWarning("Conflicto al actualizar entidad {Id}: {Error}", model.Id, responseContent);
                    return false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Entidad {Id} no encontrada para actualizar", model.Id);
                    return false;
                }
                else
                {
                    _logger.LogError("Error al actualizar entidad {Id}: {StatusCode} - {Error}",
                        model.Id, response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar entidad {model.Id}");
                return false;
            }
        }

        // Eliminar entidad
        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/entidad/{id}";
                _logger.LogInformation("=== INTENTANDO ELIMINAR ENTIDAD ===");
                _logger.LogInformation("URL: {Url}", url);
                _logger.LogInformation("ID: {Id}", id);

                var response = await _httpClient.DeleteAsync(url);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta del servidor: {ResponseContent}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<EntidadResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Eliminación exitosa. Código: {Codigo}, Mensaje: {Descripcion}",
                        result?.Codigo, result?.Descripcion);
                    return result?.Codigo == 0;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning("No se puede eliminar la entidad {Id}: {Error}", id, responseContent);
                    return false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Entidad {Id} no encontrada para eliminar", id);
                    return false;
                }
                else
                {
                    _logger.LogWarning("Error en eliminación. StatusCode: {StatusCode}, Detalle: {Error}",
                        response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar entidad {id}");
                return false;
            }
        }

        // Buscar entidades por identificador o nombre
        public async Task<List<EntidadViewModel>?> BuscarAsync(string termino)
        {
            try
            {
                var todos = await ListarTodosAsync() ?? new List<EntidadViewModel>();

                if (!string.IsNullOrWhiteSpace(termino))
                {
                    termino = termino.ToLower().Trim();
                    todos = todos.Where(e =>
                        (e.Identificador?.ToLower()?.Contains(termino) ?? false) ||
                        (e.Nombre?.ToLower()?.Contains(termino) ?? false)
                    ).ToList();

                    _logger.LogInformation("Búsqueda de entidades: término '{Termino}', resultados: {Count}",
                        termino, todos.Count);
                }

                return todos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar entidades con término '{Termino}'", termino);
                return new List<EntidadViewModel>();
            }
        }

        // Método adicional para verificar disponibilidad de identificador
        public async Task<bool> VerificarIdentificadorDisponibleAsync(string identificador, int? idExcluir = null)
        {
            try
            {
                var entidades = await ListarTodosAsync();
                if (entidades == null) return true;

                return !entidades.Any(e =>
                    e.Identificador?.Equals(identificador, StringComparison.OrdinalIgnoreCase) == true &&
                    (idExcluir == null || e.Id != idExcluir));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad de identificador {Identificador}", identificador);
                return false;
            }
        }
    }
}