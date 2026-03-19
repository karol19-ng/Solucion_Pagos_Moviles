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

                _logger.LogInformation("Token obtenido exitosamente. Longitud: {Length}", token.Length);
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
                    var result = JsonSerializer.Deserialize<EntidadResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Entidad;
                }

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

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("=== INICIANDO CREACIÓN DE ENTIDAD ===");
                _logger.LogInformation("JSON enviado: {Json}", json);

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
                        _logger.LogInformation("Entidad creada exitosamente. ID: {EntidadId}", result.Entidad?.Id);
                        return (true, "Entidad creada exitosamente", result.Entidad?.Id);
                    }
                    else
                    {
                        return (false, result?.Descripcion ?? "Error al crear la entidad", null);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    return (false, "Ya existe una entidad con ese identificador", null);
                }
                else
                {
                    _logger.LogWarning("Error en creación. StatusCode: {StatusCode}", response.StatusCode);
                    _logger.LogWarning("Detalle: {Error}", responseContent);
                    return (false, $"Error del servidor: {response.StatusCode}", null);
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

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"gateway/api/entidad/{model.Id}";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

                var response = await _httpClient.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<EntidadResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Codigo == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar entidad {model.Id}");
                throw;
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

                    _logger.LogInformation("Eliminación exitosa. Código: {Codigo}", result?.Codigo);
                    return result?.Codigo == 0;
                }
                else
                {
                    _logger.LogWarning("Error en eliminación. StatusCode: {StatusCode}", response.StatusCode);
                    _logger.LogWarning("Detalle: {Error}", responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar entidad {id}");
                return false;
            }
        }

        // Buscar entidades por identificador
        public async Task<List<EntidadViewModel>?> BuscarAsync(string termino)
        {
            try
            {
                var todos = await ListarTodosAsync() ?? new List<EntidadViewModel>();

                if (!string.IsNullOrWhiteSpace(termino))
                {
                    termino = termino.ToLower();
                    todos = todos.Where(e =>
                        (e.Identificador?.ToLower()?.Contains(termino) ?? false) ||
                        (e.Nombre?.ToLower()?.Contains(termino) ?? false)
                    ).ToList();
                }

                return todos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar entidades");
                return new List<EntidadViewModel>();
            }
        }
    }
}