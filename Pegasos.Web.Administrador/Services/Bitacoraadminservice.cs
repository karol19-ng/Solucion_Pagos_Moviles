
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Security.Claims;
    using Pegasos.Web.Administrador.Models;
    using global::Pegasos.Web.Administrador.Models;

    namespace Pegasos.Web.Administrador.Services
    {
        public class BitacoraAdminService : IBitacoraAdminService
        {
            private readonly HttpClient _httpClient;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly ILogger<BitacoraAdminService> _logger;

            public BitacoraAdminService(
                HttpClient httpClient,
                IHttpContextAccessor httpContextAccessor,
                ILogger<BitacoraAdminService> logger)
            {
                _httpClient = httpClient;
                _httpContextAccessor = httpContextAccessor;
                _logger = logger;
            }

            private void AgregarTokenAlHeader()
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user?.Identity?.IsAuthenticated != true)
                    throw new UnauthorizedAccessException("Usuario no autenticado");

                var token = user.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;

                    _logger.LogWarning("TOKEN BITACORA: {Token}",
               string.IsNullOrEmpty(token) ? "NULL/VACIO" : token.Substring(0, 30) + "...");
            if (string.IsNullOrEmpty(token))
                    throw new UnauthorizedAccessException("No hay token de autenticación disponible");

                _httpClient.DefaultRequestHeaders.Authorization = null;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            public async Task<List<BitacoraItemViewModel>> ConsultarTransaccionesAsync(DateTime? fecha)
            {
                try
                {
                    AgregarTokenAlHeader();

                    var url = fecha.HasValue
                        ? $"gateway/bitacora?fecha={fecha.Value:yyyy-MM-dd}"
                        : "gateway/bitacora";

                    _logger.LogInformation("Consultando bitácora: {Url}", url);

                    var response = await _httpClient.GetAsync(url);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("Respuesta bitácora - Status: {StatusCode}", response.StatusCode);

                    if (response.IsSuccessStatusCode)
                    {
                        var resultados = JsonSerializer.Deserialize<List<BitacoraItemViewModel>>(
                            responseContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );
                        return resultados ?? new List<BitacoraItemViewModel>();
                    }

                    _logger.LogWarning("Error bitácora: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new List<BitacoraItemViewModel>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al consultar bitácora");
                    return new List<BitacoraItemViewModel>();
                }
            }
        }
    }    
    

