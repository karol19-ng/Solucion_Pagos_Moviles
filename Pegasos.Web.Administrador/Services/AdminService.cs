

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Pegasos.WEB.Admin.Models;

namespace Pegasos.WEB.Admin.Services
{
    public class AdminService : IAdminService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminService> _logger;

        public AdminService(HttpClient httpClient, ILogger<AdminService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // SA12 - Obtener reporte de transacciones diarias desde el Gateway
        public async Task<ReporteTransaccionViewModel?> ObtenerReporteTransaccionesAsync(DateTime fecha, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Formatear fecha como yyyy-MM-dd para el query string
                var fechaStr = fecha.ToString("yyyy-MM-dd");
                var url = $"gateway/transactions/reporte?fecha={fechaStr}";

                _logger.LogInformation("Consultando reporte de transacciones: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Status: {Status} | Respuesta: {Response}",
                    response.StatusCode, responseJson);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Reporte falló con status {Status}", response.StatusCode);
                    return null;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<ReporteTransaccionViewModel>(responseJson, options);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reporte de transacciones");
                return null;
            }
        }
    }
}