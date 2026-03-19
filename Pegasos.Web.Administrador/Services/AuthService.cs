using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;

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

                var jsonManual = $"{{\"usuario\":\"{username}\",\"password\":\"{password}\"}}";
                var content = new StringContent(jsonManual, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando login para usuario: {Username}", username);

                var response = await _httpClient.PostAsync("auth/login", content);

                if (response.IsSuccessStatusCode)
                {
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
