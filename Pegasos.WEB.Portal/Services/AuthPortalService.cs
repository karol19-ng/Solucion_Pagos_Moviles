using System.Text;
using System.Text.Json;
using Pegasos.WEB.Portal.Models.ViewModels;

namespace Pegasos.WEB.Portal.Services
{
    public class AuthPortalService : IAuthPortalService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthPortalService> _logger;

        public AuthPortalService(HttpClient httpClient, ILogger<AuthPortalService> logger)
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

                _logger.LogInformation("Enviando login a gateway: auth/login");

                var response = await _httpClient.PostAsync("auth/login", content);

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta del gateway: Status={status}, Body={body}",
                    response.StatusCode, responseJson);

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<AuthResult>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning("Login fallido: {Error}", responseJson);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                return null;
            }
        }

        public async Task<bool> ExtendSessionAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

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