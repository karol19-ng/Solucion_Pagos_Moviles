using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace API_Gateway.Middleware
{
    public class GatewayAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GatewayAuthenticationMiddleware> _logger;


        //rutas que no requieren autenticacion 

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
        
        };//end of rutas publicas

        public GatewayAuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<GatewayAuthenticationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        { 
            var path =context.Request.Path.Value;
            if (path.Contains("swagger") || path.Contains("login"))
            {
                await _next(context);
                return;
            }
            //verificacion de ruta si es publica 
            if (IsPublicRoute(path))
            {
                _logger.LogDebug("Ruta publica accedida:{Path}", path);
                await _next(context);
                return;
            }

            //verificacion sin token no ingresa

            var authHeader=context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader)||!authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("GTW2:Acceso denegado -Sin Token.Path:{Path}", path);
                await RespondUnauthorized(context, "Token de autenticacion requerido");
                return;

            }

            var token =authHeader.Substring("Bearer ".Length).Trim();

            //validacion token
            if (!ValidateToken(token, out var error))
            { 
                _logger.LogWarning("GTW2:Acceso Denegado-Token Invalido.Path:{Path}.Error:{Error}", path, error);
                await RespondUnauthorized(context, $"Token invalido: {error}");
                return;
            }

            _logger.LogDebug("GTW2: Token valido.Acceso concedido.Path:{Path}", path);
            await _next(context);

        }//fin de InvokeAsync

        private bool IsPublicRoute(string path)
        {
            return PublicRoutes.Any(r => path.StartsWith(r.ToLower()));
        }

        private bool ValidateToken(string token, out string error) 
        {
            error = string.Empty;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

                var validationParameters = new
                    Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };


                handler.ValidateToken(token, validationParameters, out _);
                return true;
            }//fin del try

            catch (SecurityTokenInvalidAlgorithmException)
            {

                error = "Su token ha expirado. Por favor, inicie sesión nuevamente.";
                return false;

            }//fin del catch
            catch (SecurityTokenInvalidSignatureException)
            {
                error = "La firma del token es inválida. Por favor, inicie sesión nuevamente.";
                return false;
            }
            catch (Exception ex)
            { 
                error= ex.Message;
                return false;

            }
        }// end of ValidateToken


        private static async Task RespondUnauthorized(HttpContext context, string message)
        {

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new
            {
                StatusCode = 401,
                Error = "Unauthorized",
                Message = message,
                Timestamp= DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(response);

        }

    }//end of class


}// end of namespace
