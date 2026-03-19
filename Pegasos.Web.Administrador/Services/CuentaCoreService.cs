using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public class CuentaCoreService : ICuentaCoreService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CuentaCoreService> _logger;
        private readonly IClienteCoreService _clienteService;

        public CuentaCoreService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CuentaCoreService> logger,
            IClienteCoreService clienteService)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _clienteService = clienteService;
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

        // Listar todas las cuentas
        public async Task<List<CuentaCoreViewModel>?> ListarTodosAsync()
        {
            try
            {
                AgregarTokenAlHeader();

                var url = "gateway/api/CoreAccount";
                _logger.LogInformation("Llamando a Gateway 1: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Respuesta exitosa de Gateway 1");

                    var result = JsonSerializer.Deserialize<CuentaCoreResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Cuentas ?? new List<CuentaCoreViewModel>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gateway 1: Token no autorizado (401). Detalle: {Error}", error);
                    throw new UnauthorizedAccessException($"Token rechazado por Gateway 1: {error}");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Gateway 1: Ruta no encontrada - 404. Verificar configuración de Ocelot");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error al listar cuentas: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return new List<CuentaCoreViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar cuentas");
                throw;
            }
        }

        // Listar por llave primaria
        public async Task<CuentaCoreViewModel?> ObtenerPorIdAsync(int id)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/CoreAccount/{id}";
                _logger.LogInformation("Llamando a Gateway 1: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CuentaCoreResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Cuenta;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cuenta {id}");
                throw;
            }
        }

        // Listar por cliente (por identificación)
        public async Task<List<CuentaCoreViewModel>?> ObtenerPorClienteAsync(string identificacionCliente)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/CoreAccount/cliente/{identificacionCliente}";
                _logger.LogInformation("Llamando a Gateway 1: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CuentasPorClienteResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Cuentas ?? new List<CuentaCoreViewModel>();
                }

                return new List<CuentaCoreViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cuentas por cliente {identificacionCliente}");
                throw;
            }
        }

        // Listar por cliente (por ID)
        public async Task<List<CuentaCoreViewModel>?> ObtenerPorClienteIdAsync(int clienteId)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/CoreAccount/cliente-id/{clienteId}";
                _logger.LogInformation("Llamando a Gateway 1: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CuentasPorClienteResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Cuentas ?? new List<CuentaCoreViewModel>();
                }

                return new List<CuentaCoreViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cuentas por cliente ID {clienteId}");
                throw;
            }
        }

        // Crear cuenta
        public async Task<(bool Exito, string Mensaje, int? CuentaId)> CrearAsync(CrearCuentaCoreViewModel model)
        {
            try
            {
                // Primero verificar que el cliente existe
                var cliente = await _clienteService.ObtenerPorIdentificacionAsync(model.ClienteIdentificacion);
                if (cliente == null)
                {
                    return (false, "El cliente no existe en el sistema", null);
                }

                AgregarTokenAlHeader();

                // Crear objeto para enviar al API
                var requestModel = new
                {
                    clienteId = cliente.Id,
                    //tipoCuenta = model.TipoCuenta
                    // El número de cuenta se autogenera en el backend
                };

                var json = JsonSerializer.Serialize(requestModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("=== INICIANDO CREACIÓN DE CUENTA ===");
                _logger.LogInformation("JSON enviado: {Json}", json);
                _logger.LogInformation("Cliente ID: {ClienteId}, Tipo: {Tipo}", cliente.Id); //, model.TipoCuenta);

                var response = await _httpClient.PostAsync("gateway/api/CoreAccount", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta del servidor: {ResponseContent}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<CuentaCoreResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Codigo == 0)
                    {
                        _logger.LogInformation("Cuenta creada exitosamente. ID: {CuentaId}", result.Cuenta?.Id);
                        return (true, "Cuenta creada exitosamente", result.Cuenta?.Id);
                    }
                    else
                    {
                        return (false, result?.Descripcion ?? "Error al crear la cuenta", null);
                    }
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
                _logger.LogError(ex, "Error al crear cuenta");
                return (false, $"Error al crear cuenta: {ex.Message}", null);
            }
        }

        // Editar cuenta
        public async Task<bool> ActualizarAsync(EditarCuentaCoreViewModel model)
        {
            try
            {
                AgregarTokenAlHeader();

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"gateway/api/CoreAccount/{model.Id}";
                _logger.LogInformation("Llamando a Gateway 1: {Url}", url);

                var response = await _httpClient.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CuentaCoreResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Codigo == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar cuenta {model.Id}");
                throw;
            }
        }

        // Eliminar cuenta
        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/CoreAccount/{id}";
                _logger.LogInformation("=== INTENTANDO ELIMINAR CUENTA ===");
                _logger.LogInformation("URL: {Url}", url);
                _logger.LogInformation("ID: {Id}", id);

                var response = await _httpClient.DeleteAsync(url);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta del servidor: {ResponseContent}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<CuentaCoreResponse>(responseContent, new JsonSerializerOptions
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
                _logger.LogError(ex, $"Error al eliminar cuenta {id}");
                return false;
            }
        }

        // Obtener tipos de cuenta
        public async Task<List<string>?> ObtenerTiposCuentaAsync()
        {
            try
            {
                AgregarTokenAlHeader();

                var url = "gateway/api/CoreAccount/tipos-cuenta";
                _logger.LogInformation("Llamando a Gateway 1: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return new List<string> { "CORRIENTE", "AHORROS" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de cuenta");
                return new List<string> { "CORRIENTE", "AHORROS" };
            }
        }
    }
}