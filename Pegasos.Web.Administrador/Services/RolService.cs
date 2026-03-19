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

                var apiUrl = "https://localhost:7258/api/rol";
                _logger.LogInformation("Listando roles desde: {Url}", apiUrl);

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Respuesta: {Json}", json);

                    var roles = JsonSerializer.Deserialize<List<RolViewModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return roles ?? new List<RolViewModel>();
                }

                _logger.LogWarning("Error al listar roles: {StatusCode}", response.StatusCode);
                return new List<RolViewModel>();
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

                var apiUrl = $"https://localhost:7258/api/rol/{id}";
                _logger.LogInformation("Obteniendo rol {Id} desde: {Url}", id, apiUrl);

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var rol = JsonSerializer.Deserialize<RolViewModel>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return rol;
                }

                return null;
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

                // Primero obtener todas las pantallas
                var pantallasUrl = "https://localhost:7258/api/screen";
                _logger.LogInformation("Obteniendo pantallas desde: {Url}", pantallasUrl);

                var response = await _httpClient.GetAsync(pantallasUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var pantallas = JsonSerializer.Deserialize<List<PantallaViewModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (pantallas == null) return new List<PantallaAsignadaViewModel>();

                    // Si hay un rolId, obtener sus pantallas
                    List<int> pantallasDelRol = new List<int>();
                    if (rolId > 0)
                    {
                        var rol = await ObtenerPorIdAsync(rolId);
                        if (rol != null && rol.Pantallas != null)
                        {
                            pantallasDelRol = rol.Pantallas.Select(p => p.Id).ToList();
                        }
                    }

                    // Crear lista con asignación
                    var resultado = pantallas.Select(p => new PantallaAsignadaViewModel
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Asignada = rolId > 0 ? pantallasDelRol.Contains(p.Id) : false
                    }).ToList();

                    return resultado;
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

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("=== CREANDO ROL ===");
                _logger.LogInformation("JSON enviado: {Json}", json);

                var apiUrl = "https://localhost:7258/api/rol";
                var response = await _httpClient.PostAsync(apiUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseContent);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol");
                return false;
            }
        }

        public async Task<bool> ActualizarAsync(EditarRolViewModel model)
        {
            try
            {
                AgregarTokenAlHeader();

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiUrl = $"https://localhost:7258/api/rol/{model.Id}";
                _logger.LogInformation("Actualizando rol {Id}: {Json}", model.Id, json);

                var response = await _httpClient.PutAsync(apiUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseContent);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar rol {model.Id}");
                return false;
            }
        }

        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                AgregarTokenAlHeader();

                var apiUrl = $"https://localhost:7258/api/rol/{id}";
                _logger.LogInformation("Eliminando rol {Id} desde: {Url}", id, apiUrl);

                var response = await _httpClient.DeleteAsync(apiUrl);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseContent);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar rol {id}");
                return false;
            }
        }
    }
}