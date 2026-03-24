using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
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

        public async Task<List<ClienteCoreViewModel>?> ListarTodosAsync()
        {
            try
            {
                AgregarTokenAlHeader();

                var url = "gateway/api/CoreClient";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Respuesta JSON: {Json}", json);

                    var result = JsonSerializer.Deserialize<ClienteCoreResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Clientes != null)
                    {
                        _logger.LogInformation("=== CLIENTES RECIBIDOS ===");
                        foreach (var c in result.Clientes)
                        {
                            _logger.LogInformation("Cliente - ID: {Id}, Nombre: {Nombre}, Identificacion: {Identificacion}, Telefono: {Telefono}, FechaNac: {FechaNac}",
                                c.Id, c.NombreCompleto, c.Identificacion, c.Telefono, c.FechaNacimiento);
                        }
                    }

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
                AgregarTokenAlHeader();

                _logger.LogInformation("=== OBTENIENDO CLIENTE POR ID ===");
                _logger.LogInformation("ID solicitado: {Id}", id);
                _logger.LogInformation("BaseAddress del HttpClient: {BaseAddress}", _httpClient.BaseAddress);

                var url = $"gateway/api/CoreClient/{id}";
                _logger.LogInformation("URL: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Exito: {Json}", json);

                    var result = JsonSerializer.Deserialize<ClienteCoreResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Cliente != null)
                    {
                        _logger.LogInformation("Cliente encontrado - ID: {Id}, Nombre: {Nombre}, Telefono: {Telefono}, FechaNac: {FechaNac}",
                            result.Cliente.Id, result.Cliente.NombreCompleto, result.Cliente.Telefono, result.Cliente.FechaNacimiento);
                    }

                    return result?.Cliente;
                }

                _logger.LogWarning("❌ Falló obtener cliente {Id}. StatusCode: {StatusCode}", id, response.StatusCode);
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
                AgregarTokenAlHeader();

                var url = $"gateway/api/CoreClient/identificacion/{identificacion}";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

                var response = await _httpClient.GetAsync(url);

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
                throw;
            }
        }

        public async Task<bool> CrearAsync(CrearClienteCoreViewModel model)
        {
            try
            {
                AgregarTokenAlHeader();

                // Crea objeto con todos los campos  
                var request = new
                {
                    tipoIdentificacion = model.TipoIdentificacion,
                    identificacion = model.Identificacion,
                    nombreCompleto = model.NombreCompleto,
                    telefono = model.Telefono,                    
                    fechaNacimiento = model.FechaNacimiento.ToString("yyyy-MM-dd")  
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("=== INICIANDO CREACIÓN DE CLIENTE ===");
                _logger.LogInformation("JSON enviado: {Json}", json);
                _logger.LogInformation("BaseAddress del HttpClient: {BaseAddress}", _httpClient.BaseAddress);

                var response = await _httpClient.PostAsync("gateway/api/CoreClient", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta HTTP: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta del servidor: {ResponseContent}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ClienteCoreResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Creación exitosa");
                    return result?.Codigo == 0;
                }
                else
                {
                    _logger.LogWarning("Error en creación. StatusCode: {StatusCode}", response.StatusCode);
                    _logger.LogWarning("Detalle: {Error}", responseContent);
                    return false;
                }
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
                AgregarTokenAlHeader();

                _logger.LogInformation("=== ACTUALIZANDO CLIENTE ===");
                _logger.LogInformation("Modelo recibido - Id: {Id}, Nombre: {Nombre}, Identificacion: {Identificacion}, TipoId: {TipoId}, Telefono: {Telefono}, FechaNac: {FechaNac}, EstadoId: {EstadoId}",
                    model.Id, model.NombreCompleto, model.Identificacion, model.TipoIdentificacion, model.Telefono, model.FechaNacimiento, model.EstadoId);

                // Crea un objeto con todos los campos 
                var request = new
                {
                    id = model.Id,
                    tipoIdentificacion = model.TipoIdentificacion,
                    identificacion = model.Identificacion,
                    nombreCompleto = model.NombreCompleto,
                    telefono = model.Telefono,                    
                    fechaNacimiento = model.FechaNacimiento.ToString("yyyy-MM-dd"),  
                    estadoId = model.EstadoId
                };

                var json = JsonSerializer.Serialize(request);
                _logger.LogInformation("JSON enviado: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var url = $"gateway/api/CoreClient/{model.Id}";

                _logger.LogInformation("URL: {Url}", url);

                var response = await _httpClient.PutAsync(url, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ClienteCoreResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    _logger.LogInformation("Actualización exitosa. Código: {Codigo}", result?.Codigo);
                    return result?.Codigo == 0;
                }

                _logger.LogWarning("Error en actualización. StatusCode: {StatusCode}", response.StatusCode);
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
                AgregarTokenAlHeader();

                var url = $"gateway/api/CoreClient/{id}";
                _logger.LogInformation("=== INTENTANDO ELIMINAR CLIENTE ===");
                _logger.LogInformation("URL: {Url}", url);
                _logger.LogInformation("ID: {Id}", id);

                var response = await _httpClient.DeleteAsync(url);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Código de respuesta: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Respuesta del servidor: {ResponseContent}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ClienteCoreResponse>(responseContent, new JsonSerializerOptions
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
                        _logger.LogWarning("Cliente no encontrado (404)");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        _logger.LogWarning("Bad Request - posiblemente tiene cuentas asociadas");
                    }

                    return false;
                }
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
                AgregarTokenAlHeader();

                var url = "gateway/api/CoreClient/tipos-identificacion";
                _logger.LogInformation("Llamando a Gateway: {Url}", url);

                var response = await _httpClient.GetAsync(url);

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