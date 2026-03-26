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
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Respuesta JSON recibida: {Json}", json);

                    var result = JsonSerializer.Deserialize<CuentaCoreResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Cuentas != null)
                    {
                        _logger.LogInformation("=== CUENTAS RECIBIDAS ===");
                        foreach (var c in result.Cuentas)
                        {
                            _logger.LogInformation("Cuenta - ID: {Id}, Número: {Numero}, Cliente: {Cliente}, Tipo: {Tipo}, Saldo: {Saldo}, Estado: {Estado}",
                                c.Id, c.NumeroCuenta, c.ClienteNombre, c.TipoCuenta, c.Saldo, c.EstadoId);
                        }
                    }

                    return result?.Cuentas ?? new List<CuentaCoreViewModel>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error al listar cuentas: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return new List<CuentaCoreViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar cuentas");
                return new List<CuentaCoreViewModel>();
            }
        }

        // Listar por llave primaria
        public async Task<CuentaCoreViewModel?> ObtenerPorIdAsync(int id)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/CoreAccount/{id}";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

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
                return null;
            }
        }

        // Listar por cliente (por identificación)
        public async Task<List<CuentaCoreViewModel>?> ObtenerPorClienteAsync(string identificacionCliente)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/CoreAccount/cliente/{identificacionCliente}";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

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
                return new List<CuentaCoreViewModel>();
            }
        }

        // Listar por cliente (por ID)
        public async Task<List<CuentaCoreViewModel>?> ObtenerPorClienteIdAsync(int clienteId)
        {
            try
            {
                AgregarTokenAlHeader();

                var url = $"gateway/api/CoreAccount/cliente-id/{clienteId}";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

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
                return new List<CuentaCoreViewModel>();
            }
        }

        public async Task<(bool Exito, string Mensaje, int? CuentaId)> CrearAsync(CrearCuentaCoreViewModel model)
        {
            try
            {
                // Validación adicional
                if (model == null || string.IsNullOrWhiteSpace(model.ClienteIdentificacion))
                {
                    return (false, "Debe proporcionar una identificación de cliente", null);
                }

                // ✅ Verificar que TipoCuenta no esté vacío
                if (string.IsNullOrWhiteSpace(model.TipoCuenta))
                {
                    _logger.LogWarning("❌ TipoCuenta está vacío en CrearAsync");
                    return (false, "Debe seleccionar un tipo de cuenta", null);
                }

                ClienteCoreViewModel? cliente = null;

                // Intentar buscar por ID si es numérico
                if (int.TryParse(model.ClienteIdentificacion, out int clienteId))
                {
                    cliente = await _clienteService.ObtenerPorIdAsync(clienteId);
                }

                // Si no encuentra por ID, buscar por identificación
                if (cliente == null)
                {
                    cliente = await _clienteService.ObtenerPorIdentificacionAsync(model.ClienteIdentificacion);
                }

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente no encontrado: {Valor}", model.ClienteIdentificacion);
                    return (false, $"El cliente con identificación/ID '{model.ClienteIdentificacion}' no existe. Verifique el valor e intente nuevamente.", null);
                }

                _logger.LogInformation("Cliente encontrado: ID={ClienteId}, Nombre={Nombre}, Identificacion={Identificacion}",
                    cliente.Id, cliente.NombreCompleto, cliente.Identificacion);

                AgregarTokenAlHeader();

                // Enviar el ID del cliente y el tipo de cuenta
                var requestModel = new
                {
                    clienteId = cliente.Id,
                    tipoCuenta = model.TipoCuenta.ToUpper()  // Asegurar mayúsculas
                };

                var json = JsonSerializer.Serialize(requestModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando solicitud de creación de cuenta para cliente ID: {ClienteId}, Tipo: {TipoCuenta}",
                    cliente.Id, model.TipoCuenta);
                _logger.LogInformation("JSON enviado: {Json}", json);

                var response = await _httpClient.PostAsync("gateway/api/CoreAccount", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta del servidor: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<CuentaCoreResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Codigo == 0 && result.Cuenta != null)
                    {
                        _logger.LogInformation("Cuenta creada exitosamente. ID: {CuentaId}, Número: {NumeroCuenta}",
                            result.Cuenta.Id, result.Cuenta.NumeroCuenta);
                        return (true, $"✅ Cuenta creada exitosamente. Número: {result.Cuenta.NumeroCuenta}", result.Cuenta.Id);
                    }
                    else
                    {
                        return (false, result?.Descripcion ?? "Error al crear la cuenta", null);
                    }
                }
                else
                {
                    _logger.LogWarning("Error en creación. StatusCode: {StatusCode}, Respuesta: {Response}",
                        response.StatusCode, responseContent);
                    return (false, $"Error al crear la cuenta: {response.StatusCode} - {responseContent}", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuenta");
                return (false, $"Error inesperado: {ex.Message}", null);
            }
        }

        // Editar cuenta
        public async Task<bool> ActualizarAsync(EditarCuentaCoreViewModel model)
        {
            try
            {
                AgregarTokenAlHeader();

                var requestModel = new
                {
                    id = model.Id,
                    tipoCuenta = model.TipoCuenta,
                    estadoId = model.EstadoId
                };

                var json = JsonSerializer.Serialize(requestModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"gateway/api/CoreAccount/{model.Id}";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);
                _logger.LogInformation("JSON enviado: {Json}", json);

                var response = await _httpClient.PutAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<CuentaCoreResponse>(responseContent, new JsonSerializerOptions
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
                return false;
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

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("Cuenta no encontrada (404)");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        _logger.LogWarning("Bad Request - posiblemente tiene movimientos asociados");
                    }

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
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tipos = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Tipos de cuenta obtenidos: {Tipos}", string.Join(", ", tipos ?? new List<string>()));
                    return tipos ?? new List<string> { "CORRIENTE", "AHORROS" };
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