using System.Text;
using System.Text.Json;
using Pegasos.Web.Administrador.Models;


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

        public async Task<List<PantallaResponse>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("screen");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PantallaResponse>>(json, _jsonOptions) ?? new();
        }

        public async Task<PantallaResponse> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"screen/{id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PantallaResponse>(json, _jsonOptions);
        }

        public async Task<PantallaResponse> CreateAsync(PantallaRequest request)
        {
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
            var response = await _httpClient.DeleteAsync($"screen/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
