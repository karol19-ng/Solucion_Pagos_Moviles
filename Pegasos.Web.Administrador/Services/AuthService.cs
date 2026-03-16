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

                // 1. CREAMOS EL JSON MANUALMENTE (Exactamente como lo pones en Swagger)
                // Usamos minúsculas porque Swagger suele ser case-insensitive o usa camelCase
                var jsonManual = $"{{\"usuario\":\"{username}\",\"password\":\"{password}\"}}";

                // 2. Definimos el contenido con el Header exacto
                var content = new StringContent(jsonManual, Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando JSON Crudo: {json}", jsonManual);

                // 3. Enviamos al puerto 5200 (Gateway) ya que dices que ya no da problemas
                var response = await _httpClient.PostAsync("auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AuthResult>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Respuesta final del Micro: {Error}", error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexión");
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
