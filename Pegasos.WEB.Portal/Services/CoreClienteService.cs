using System.Text.Json;
using System.Net.Http.Headers;
using Pegasos.WEB.Portal.Models.ViewModels;

namespace Pegasos.WEB.Portal.Services
{
    public class CoreClienteService : ICoreClienteService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CoreClienteService> _logger;

        public CoreClienteService(HttpClient httpClient, ILogger<CoreClienteService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ClienteInfoViewModel?> ObtenerInfoClienteAsync(string identificacion, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                _logger.LogInformation("Obteniendo info del cliente con identificación: {Identificacion}", identificacion);

                var response = await _httpClient.GetAsync($"core/client?identificacion={identificacion}");

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta core: Status={status}, Body={body}",
                    response.StatusCode, responseJson);

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<ClienteInfoViewModel>(responseJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo info del cliente");
                return null;
            }
        }
    }
}