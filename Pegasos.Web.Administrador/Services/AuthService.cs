using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Pegasos.Web.Administrador.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System; // Agregar esto para DateTime

namespace Pegasos.Web.Administrador.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AuthResult?> LoginAsync(string username, string password)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();

                // 1. Enviar credenciales al gateway
                var jsonManual = $"{{\"usuario\":\"{username}\",\"password\":\"{password}\"}}";
                var content = new StringContent(jsonManual, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando login para usuario: {Username}", username);

                var response = await _httpClient.PostAsync("auth/login", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("RESPUESTA COMPLETA DEL GATEWAY: StatusCode: {StatusCode}, Body: {Body}",
                    response.StatusCode, responseJson);

                if (response.IsSuccessStatusCode)
                {
                    // 2. Deserializar a DTO que coincide con la respuesta del gateway
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var dto = JsonSerializer.Deserialize<AuthResponseDto>(responseJson, options);

                    if (dto == null || string.IsNullOrEmpty(dto.AccessToken))
                    {
                        _logger.LogWarning("No se pudo deserializar el token o el token está vacío");
                        return null;
                    }

                    _logger.LogInformation("Token recibido correctamente. Longitud: {Length}", dto.AccessToken.Length);

                    // 3. Decodificar el JWT para obtener los claims del usuario
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(dto.AccessToken);

                    // Extraer claims del token
                    var usuarioId = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value ?? "0";
                    var nombreCompleto = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value ?? username;
                    var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";

                    _logger.LogInformation("Claims extraídos del token - UsuarioId: {UsuarioId}, Nombre: {Nombre}, Email: {Email}",
                        usuarioId, nombreCompleto, email);

                    // 4. Crear el AuthResult con todos los datos
                    var resultado = new AuthResult
                    {
                        AccessToken = dto.AccessToken,
                        RefreshToken = dto.RefreshToken ?? "",
                        ExpiresIn = dto.ExpiresIn ?? "", 
                        UsuarioId = int.TryParse(usuarioId, out var id) ? id : 0,
                        NombreCompleto = nombreCompleto
                    };

                    _logger.LogInformation("Login exitoso para usuario: {Nombre}, ID: {UsuarioId}",
                        resultado.NombreCompleto, resultado.UsuarioId);

                    return resultado;
                }
                else
                {
                    _logger.LogWarning("Error en login. StatusCode: {StatusCode}, Error: {Error}",
                        response.StatusCode, responseJson);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexión en LoginAsync para usuario {Username}", username);
                    var responseJson = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Respuesta del login recibida: {Response}", responseJson);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = null // No cambiar nombres
                    };

                    var result = JsonSerializer.Deserialize<AuthResult>(responseJson, options);

                    if (result != null)
                    {
                        _logger.LogInformation("Token recibido: {TokenPresent} - Longitud: {Length}",
                            string.IsNullOrEmpty(result.access_token) ? "VACÍO" : "OK",
                            result.access_token?.Length ?? 0);

                        _logger.LogInformation("UsuarioID recibido: {UsuarioId}", result.usuarioID);
                    }

                    return result;
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login fallido - StatusCode: {StatusCode}, Error: {Error}",
                    response.StatusCode, error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexión en login");
                return null;
            }
        }

        public async Task<bool> ExtendSessionAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.PostAsync("auth/refresh", new StringContent("{}"));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}