using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_Gateway.Middleware
{
    public class GatewayAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GatewayAuthenticationMiddleware> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private static readonly string[] PublicRoutes = new[]
        {
            "/gateway/auth/login",
            "/gateway/auth/refresh",
            "/gateway/auth/validate",
            "/admin/Auth/Login",
            "/admin/Account/Login",
            "/portal/Auth/Login",
            "/portal/Account/Login",
            "/admin/css/",
            "/admin/js/",
            "/admin/images/",
            "/portal/css/",
            "/portal/js/",
            "/portal/images/",
            "/favicon.ico",
            "/css/",
            "/js/",
            "/swagger",
            "/index.html",
            "/images/"
        };

        public GatewayAuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<GatewayAuthenticationMiddleware> logger,
            IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;

            // Rutas públicas rápidas
            if (path.Contains("swagger") || path.Contains("login"))
            {
                _logger.LogInformation("Header: {Key} = {Value}", header.Key, header.Value);
            }

            // Verificar si es ruta pública
            if (IsPublicRoute(path))
            {
                _logger.LogDebug("Ruta publica accedida: {Path}", path);
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("GTW1: Acceso denegado - Sin Token. Path: {Path}", path);
                await RespondUnauthorized(context, "Token de autenticacion requerido");
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Validar token localmente
            if (ValidateTokenLocally(token, out var principal))
            {
                _logger.LogDebug("GTW1: Token válido localmente. Path: {Path}", path);

                // IMPORTANTE: Establecer el usuario con el esquema correcto
                context.User = principal;

                await _next(context);
                return;
            }

            // Si falla localmente, intentar validar con microservicio
            _logger.LogInformation("Validación local falló, intentando con microservicio para: {Path}", path);

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
                        _logger.LogInformation("Token validado por microservicio para: {Path}", path);

                        // ===== CORRECCIÓN IMPORTANTE =====
                        // Crear un ClaimsPrincipal que Ocelot reconozca como autenticado
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, "user"),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            // Agregar cualquier otro claim que necesites
                        };

                        // Usar el mismo esquema de autenticación que Ocelot espera
                        var identity = new ClaimsIdentity(claims, "GatewayAuth");  // ← ESQUEMA CORRECTO
                        var authenticatedPrincipal = new ClaimsPrincipal(identity);

                        context.User = authenticatedPrincipal;

                        await _next(context);
                        return;
                    }
                }

                _logger.LogWarning("GTW1: Token inválido según microservicio. Path: {Path}", path);
                await RespondUnauthorized(context, "Token invalido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando token con microservicio para: {Path}", path);
                await RespondUnauthorized(context, "Error validando token");
            }
        }

        private bool IsPublicRoute(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return PublicRoutes.Any(r => path.StartsWith(r, StringComparison.OrdinalIgnoreCase));
        }

        private bool ValidateTokenLocally(string token, out ClaimsPrincipal principal)
        {
            principal = null;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = false,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                principal = handler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Validación local falló: {Message}", ex.Message);
                return false;
            }
        }

        private static async Task RespondUnauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new
            {
                StatusCode = 401,
                Error = "Unauthorized",
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}