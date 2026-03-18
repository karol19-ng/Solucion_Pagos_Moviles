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

        private static readonly string[] PublicRoutes = new[]
        {

            "/gateway/auth/login",
            "/gateway/auth/refresh",
            "/admin/auth/login",
            "/admin/Auth/Login",
            "/swagger",
            "/index.html",
            "/ccs/","/js","/images","/lib"

        };// fin de rutas publicas

        public GatewayAuthMiddleware(RequestDelegate next, IConfiguration config,
            ILogger<GatewayAuthMiddleware> logger)
        {

            _next = next;
            _config = config;
            _logger = logger;

        }//fin del gateway auth middleware

        public async Task InvokeAsync(HttpContext contex)
        {

            var path = contex.Request.Path.Value?.ToLower() ?? "";
            if (path.Contains("swagger") || path.Contains("login")) 
            {
                await _next(contex);
                return;
            }

            if (IsPublicRoute(path))
            {
                await _next(contex);
                return;
            }// fin de ruta publica

            var authHeader = contex.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer"))
            {
                _logger.LogWarning("GTW2:No se ha encontrado el token de autorizacion - {path}", path);
                contex.Response.StatusCode = 401;// ERROR 401 sin token
                await contex.Response.WriteAsJsonAsync(new { error = "Unauthorized: No token provided", message = "Token requerido" });
                return;

            }//fin de inicio con bearer

            var token = authHeader.Substring("Bearer".Length).Trim();
            if (!ValidateToken(token))
            {
                _logger.LogWarning("GTW2:Token de autorizacion invalido - {path}", path);
                contex.Response.StatusCode = 401;
                await contex.Response.WriteAsJsonAsync(new { error = "Unauthorized: Invalid token", message = "Token invalido" });
                return;
            }//fin if validete

            await _next(contex);
        }

            //metodo de ruta publica
            private bool IsPublicRoute(string path)
            {

            return PublicRoutes.Any(r => path.StartsWith(r.ToLower()));

            }// fin ruta publica

        private bool ValidateToken(string token)
        {
            try 
            {
                var handler= new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

                handler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);
                return true;
            }//fin try
            catch (Exception ex)
            {
                return false;
            }//fin catch

        }//fin valide token 
    }
}

