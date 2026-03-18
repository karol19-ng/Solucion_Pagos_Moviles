using System.Text;
using System.Text.Json;
using Pegasos.Web.Administrador.Models;
using Microsoft.AspNetCore.Authentication;

namespace Pegasos.Web.Administrador.Services
{
    public class ScreenService : IScreenService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public ScreenService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Método auxiliar para agregar el token JWT a las peticiones
        private async Task AddTokenToRequestAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                // Obtener el token del claim "access_token"
                var token = context.User.FindFirst("access_token")?.Value;

                if (!string.IsNullOrEmpty(token))
                {
                    // Limpiar cualquier header Authorization existente y agregar el token
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                }
                else
                {
                    // Intentar obtener el token de la cookie de autenticación
                    var authenticateResult = await context.AuthenticateAsync();
                    if (authenticateResult.Succeeded)
                    {
                        token = authenticateResult.Principal?.FindFirst("access_token")?.Value;
                        if (!string.IsNullOrEmpty(token))
                        {
                            _httpClient.DefaultRequestHeaders.Remove("Authorization");
                            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                        }
                    }
                }
            }
        }

        public async Task<List<PantallaResponse>> GetAllAsync()
        {
            await AddTokenToRequestAsync(); // Agregar token antes de la petición

            var response = await _httpClient.GetAsync("screen");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PantallaResponse>>(json, _jsonOptions) ?? new();
        }

        public async Task<PantallaResponse> GetByIdAsync(int id)
        {
            await AddTokenToRequestAsync(); // Agregar token antes de la petición

            var response = await _httpClient.GetAsync($"screen/{id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PantallaResponse>(json, _jsonOptions);
        }

        public async Task<PantallaResponse> CreateAsync(PantallaRequest request)
        {
            await AddTokenToRequestAsync(); // Agregar token antes de la petición

            var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("screen", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PantallaResponse>(json, _jsonOptions);
        }

        public async Task<PantallaResponse> UpdateAsync(int id, PantallaRequest request)
        {
            await AddTokenToRequestAsync(); // Agregar token antes de la petición

            var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync($"screen/{id}", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PantallaResponse>(json, _jsonOptions);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await AddTokenToRequestAsync(); // Agregar token antes de la petición

            var response = await _httpClient.DeleteAsync($"screen/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}