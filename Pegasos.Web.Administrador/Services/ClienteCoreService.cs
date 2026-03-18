using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public class ClienteCoreService : IClienteCoreService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ClienteCoreService> _logger;

        public ClienteCoreService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ClienteCoreService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void AgregarToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<ClienteCoreViewModel>?> ListarTodosAsync()
        {
            try
            {
                AgregarToken();
                var response = await _httpClient.GetAsync("api/CoreClient");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ClienteCoreResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Clientes ?? new List<ClienteCoreViewModel>();
                }

                return new List<ClienteCoreViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar clientes");
                return new List<ClienteCoreViewModel>();
            }
        }

        public async Task<ClienteCoreViewModel?> ObtenerPorIdAsync(int id)
        {
            try
            {
                AgregarToken();
                var response = await _httpClient.GetAsync($"api/CoreClient/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ClienteCoreResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Cliente;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cliente {id}");
                return null;
            }
        }

        public async Task<ClienteCoreViewModel?> ObtenerPorIdentificacionAsync(string identificacion)
        {
            try
            {
                AgregarToken();
                var response = await _httpClient.GetAsync($"api/CoreClient/identificacion/{identificacion}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ClienteCoreResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Cliente;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cliente por identificación {identificacion}");
                return null;
            }
        }

        public async Task<bool> CrearAsync(CrearClienteCoreViewModel model)
        {
            try
            {
                AgregarToken();
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/CoreClient", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ClienteCoreResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Codigo == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente");
                return false;
            }
        }

        public async Task<bool> ActualizarAsync(EditarClienteCoreViewModel model)
        {
            try
            {
                AgregarToken();
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/CoreClient/{model.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ClienteCoreResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Codigo == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar cliente {model.Id}");
                return false;
            }
        }

        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                AgregarToken();
                var response = await _httpClient.DeleteAsync($"api/CoreClient/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ClienteCoreResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Codigo == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar cliente {id}");
                return false;
            }
        }

        public async Task<List<string>?> ObtenerTiposIdentificacionAsync()
        {
            try
            {
                AgregarToken();
                var response = await _httpClient.GetAsync("api/CoreClient/tipos-identificacion");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return new List<string> { "FISICA", "JURIDICA", "DIMEX", "NITE" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de identificación");
                return new List<string> { "FISICA", "JURIDICA", "DIMEX", "NITE" };
            }
        }
    }
}