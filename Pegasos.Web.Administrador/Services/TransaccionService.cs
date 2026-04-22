using Pegasos.Web.Administrador.DTOs;
using static Pegasos.Web.Administrador.DTOs.TransaccionDTOs;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Pegasos.Web.Administrador.Services
{
    public class TransaccionService : ITransaccionService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransaccionService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ReporteTransaccionDTO> ConsultarTransaccionesPorFechaAsync(DateTime fecha)
        {
            var token = await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token")
                        ?? _httpContextAccessor.HttpContext.Session.GetString("AccessToken");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var fechaStr = fecha.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"gateway/transactions/reporte?fecha={fechaStr}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ReporteTransaccionDTO>();
        }
    }
}