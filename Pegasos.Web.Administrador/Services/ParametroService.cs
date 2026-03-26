using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public class ParametroService : IParametroService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ParametroService> _logger;

        public ParametroService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ParametroService> logger)
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

        // Listar todos los parámetros
        public async Task<List<ParametroViewModel>?> ListarTodosAsync()
        {
            try
            {
                AgregarTokenAlHeader();

                var url = "gateway/api/parametro";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Respuesta exitosa de Gateway");

                    var result = JsonSerializer.Deserialize<ParametroResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Parametros ?? new List<ParametroViewModel>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gateway: Token no autorizado (401). Detalle: {Error}", error);
                    throw new UnauthorizedAccessException($"Token rechazado por Gateway: {error}");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error al listar parámetros: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return new List<ParametroViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar parámetros");
                throw;
            }
        }

        // Obtener por ID (string)
        public async Task<ParametroViewModel?> ObtenerPorIdAsync(string id)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/parametro/{id}";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ParametroResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Parametro;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener parámetro {id}");
                throw;
            }
        }

        // Crear parámetro
        public async Task<(bool Exito, string Mensaje)> CrearAsync(CrearParametroViewModel model)
        {
            try
            {
                AgregarTokenAlHeader();

                // Crear objeto para enviar al API
                var requestModel = new
                {
                    idParametro = model.Id,
                    valor = model.Valor,
                    idEstado = model.EstadoId
                };

                var json = JsonSerializer.Serialize(requestModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("=== INICIANDO CREACIÓN DE PARÁMETRO ===");
                _logger.LogInformation("JSON enviado: {Json}", json);

                var response = await _httpClient.PostAsync("gateway/api/parametro", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta del servidor: {ResponseContent}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ParametroResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Codigo == 0)
                    {
                        _logger.LogInformation("Parámetro creado exitosamente. ID: {Id}", model.Id);
                        return (true, "Parámetro creado exitosamente");
                    }
                    else
                    {
                        return (false, result?.Descripcion ?? "Error al crear el parámetro");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    return (false, $"Ya existe un parámetro con ID '{model.Id}'");
                }
                else
                {
                    _logger.LogWarning("Error en creación. StatusCode: {StatusCode}", response.StatusCode);
                    _logger.LogWarning("Detalle: {Error}", responseContent);
                    return (false, $"Error del servidor: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear parámetro");
                return (false, $"Error al crear parámetro: {ex.Message}");
            }
        }

        // Actualizar parámetro
        public async Task<bool> ActualizarAsync(EditarParametroViewModel model)
        {
            try
            {
                AgregarTokenAlHeader();

                var requestModel = new
                {
                    idParametro = model.Id,
                    valor = model.Valor,
                    idEstado = model.EstadoId
                };

                var json = JsonSerializer.Serialize(requestModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"gateway/api/parametro/{model.Id}";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

                var response = await _httpClient.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ParametroResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Codigo == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar parámetro {model.Id}");
                throw;
            }
        }

        // Eliminar parámetro
        public async Task<bool> EliminarAsync(string id)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/parametro/{id}";
                _logger.LogInformation("=== INTENTANDO ELIMINAR PARÁMETRO ===");
                _logger.LogInformation("URL: {Url}", url);
                _logger.LogInformation("ID: {Id}", id);

                var response = await _httpClient.DeleteAsync(url);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta del servidor: {ResponseContent}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ParametroResponse>(responseContent, new JsonSerializerOptions
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
                _logger.LogError(ex, $"Error al eliminar parámetro {id}");
                return false;
            }
        }

        // Buscar parámetros
        public async Task<List<ParametroViewModel>?> BuscarAsync(string termino)
        {
            try
            {
                var todos = await ListarTodosAsync() ?? new List<ParametroViewModel>();

                if (!string.IsNullOrWhiteSpace(termino))
                {
                    termino = termino.ToLower();
                    todos = todos.Where(p =>
                        (p.Id?.ToLower()?.Contains(termino) ?? false) ||
                        (p.Valor?.ToLower()?.Contains(termino) ?? false)
                    ).ToList();
                }

                return todos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar parámetros");
                return new List<ParametroViewModel>();
            }
        }
    }
}