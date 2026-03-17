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
            "/gateway/auth/login",
            "/gateway/auth/refresh",
            "/admin/auth/login",
            "/admin/Auth/Login",
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
            IHttpClientFactory httpClientFactory)  // Cambiado de HttpClient a IHttpClientFactory
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
            if (path.Contains("swagger") || path.Contains("login") || path.Contains("favicon"))
            {
                _logger.LogInformation("Ruta pública (swagger/login): {Path}", path);
                await _next(context);
                return;
            }

            // Verificar si es ruta pública por lista
            if (IsPublicRoute(path))
            {
                _logger.LogInformation("Ruta pública por lista: {Path}", path);
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("GTW2: No se ha encontrado el token de autorización - {Path}", path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized: No token provided",
                    message = "Token requerido"
                });
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Validación local del token primero
            if (ValidateTokenLocally(token))
            {
                _logger.LogInformation("Token válido localmente para: {Path}", path);
                await _next(context);
                return;
            }

            // Si la validación local falla, intentar validar con el microservicio
            _logger.LogInformation("Validación local falló, intentando con microservicio para: {Path}", path);

            try
            {
                // Crear HttpClient usando la fábrica
                var httpClient = _httpClientFactory.CreateClient();

                // Intentar validar con el microservicio de auth
                var validateRequest = new { token = token };
                var json = System.Text.Json.JsonSerializer.Serialize(validateRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Llamar al endpoint de validate del microservicio
                var validateResponse = await httpClient.PostAsync("https://localhost:7258/auth/validate", content);

                if (validateResponse.IsSuccessStatusCode)
                {
                    var result = await validateResponse.Content.ReadAsStringAsync();
                    if (result.ToLower().Contains("true"))
                    {
                        _logger.LogInformation("Token validado por microservicio para: {Path}", path);
                        await _next(context);
                        return;
                    }
                }

                _logger.LogWarning("Token inválido según microservicio para: {Path}", path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized: Invalid token",
                    message = "Token inválido"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando token con microservicio para: {Path}", path);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal Server Error",
                    message = "Error validando token"
                });
            }
        }

        private bool IsPublicRoute(string path)
        {
            return PublicRoutes.Any(r => path.StartsWith(r));
        }

        private bool ValidateTokenLocally(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                handler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Validación local falló: {Message}", ex.Message);
                return false;
            }
        }
    }
}