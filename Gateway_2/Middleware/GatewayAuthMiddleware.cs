using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Gateway_2.Middleware
{
    public class GatewayAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly ILogger<GatewayAuthMiddleware> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private static readonly string[] PublicRoutes = new[]
        {
            "/auth/login",
            "/auth/refresh",
            "/swagger",
            "/index.html",
            "/css/",
            "/js/",
            "/images/",
            "/lib/"
        };

        public GatewayAuthMiddleware(
            RequestDelegate next,
            IConfiguration config,
            ILogger<GatewayAuthMiddleware> logger,
            IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _config = config;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Rutas siempre públicas
            if (IsPublicRoute(path) || path.Contains("swagger") || path.Contains("login"))
            {
                _logger.LogInformation("Ruta pública: {Path}", path);
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("GTW2: No token - {Path}", path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Unauthorized: No token provided" });
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Intentar validar con microservicio
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var validateRequest = new { token = token };
                var json = System.Text.Json.JsonSerializer.Serialize(validateRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var validateResponse = await httpClient.PostAsync("https://localhost:7258/auth/validate", content);

                if (validateResponse.IsSuccessStatusCode)
                {
                    var result = await validateResponse.Content.ReadAsStringAsync();
                    if (result.ToLower().Contains("true"))
                    {
                        _logger.LogInformation("Token válido para: {Path}", path);

                        // === IMPORTANTE: Crear identidad para el usuario ===
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(token);

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, jwtToken.Subject ?? "user"),
                            new Claim("access_token", token)
                        };

                        // Agregar todos los claims del token
                        foreach (var claim in jwtToken.Claims)
                        {
                            claims.Add(new Claim(claim.Type, claim.Value));
                        }

                        var identity = new ClaimsIdentity(claims, "Gateway");
                        var principal = new ClaimsPrincipal(identity);

                        // ESTO ES CRÍTICO - Establecer el usuario autenticado
                        context.User = principal;

                        await _next(context);
                        return;
                    }
                }

                _logger.LogWarning("Token inválido para: {Path}", path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Unauthorized: Invalid token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando token");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { error = "Internal Server Error" });
            }
        }

        private bool IsPublicRoute(string path)
        {
            return PublicRoutes.Any(r => path.StartsWith(r));
        }
    }
}