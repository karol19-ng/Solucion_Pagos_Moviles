using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.WEB.Portal.Models.ViewModels;
using Pegasos.WEB.Portal.Services;
using System.IdentityModel.Tokens.Jwt;

namespace Pegasos.WEB.Portal.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthPortalService _authService;
        private readonly ILogger<AuthController> _logger;

        private static readonly Dictionary<string, (int Attempts, DateTime? LockoutEnd)> LoginAttempts = new();

        public AuthController(IAuthPortalService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null, bool expired = false)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            if (expired)
            {
                ViewBag.SessionExpired = true;
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginPortalViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginPortalViewModel model, string? returnUrl = null)
        {
            var username = model.Username;

            _logger.LogInformation("Intento de login para usuario: {Username}", username);

            // PTL1: Verificar bloqueo por 3 intentos fallidos
            if (IsUserLockedOut(username))
            {
                model.ErrorMessage = "Usuario bloqueado.";
                return View(model);
            }

            var result = await _authService.LoginAsync(model.Username, model.Password);

            if (result == null || string.IsNullOrEmpty(result.access_token))
            {
                RegisterFailedAttempt(username);
                int attempts = GetFailedAttempts(username);
                int remaining = 3 - attempts;

                _logger.LogWarning("Login fallido para usuario: {Username}. Intentos: {Attempts}", username, attempts);

                if (remaining <= 0)
                {
                    LockUser(username);
                    model.ErrorMessage = "Usuario bloqueado.";
                }
                else
                {
                    model.ErrorMessage = $"Credenciales incorrectas. Intentos restantes: {remaining}";
                }
                return View(model);
            }

            // Login exitoso
            ClearAttempts(username);

            // Decodificar el token JWT para extraer información del usuario
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(result.access_token);

            _logger.LogInformation("=== CLAIMS DISPONIBLES EN EL TOKEN ===");
            foreach (var claim in jwtToken.Claims)
            {
                _logger.LogInformation("Claim Type: {Type}, Value: {Value}", claim.Type, claim.Value);
            }

            // Extraer nombre completo
            var nombreCompleto = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == "nombreCompleto" ||
                c.Type == "NombreCompleto" ||
                c.Type == "name" ||
                c.Type == "unique_name" ||
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ?? username;

            // Extraer identificación
            var identificacion = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == "identificacion" ||
                c.Type == "Identificacion" ||
                c.Type == "id" ||
                c.Type == "Id" ||
                c.Type == "ID" ||
                c.Type == "nameid" ||
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ?? "";

            var email = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == "email" ||
                c.Type == "Email" ||
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value ?? "";

            var usuarioId = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == "nameid" ||
                c.Type == "sub")?.Value ?? "";

            _logger.LogInformation("=== INFORMACIÓN EXTRAÍDA ===");
            _logger.LogInformation("Nombre: {Nombre}", nombreCompleto);
            _logger.LogInformation("Identificación: {Identificacion}", identificacion);
            _logger.LogInformation("Email: {Email}", email);
            _logger.LogInformation("UsuarioId: {UsuarioId}", usuarioId);

            // Crear claims para la cookie de autenticación
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, usuarioId),
                new Claim("nombreCompleto", nombreCompleto),
                new Claim("access_token", result.access_token),
                new Claim("refresh_token", result.refresh_token ?? ""),
                new Claim("identificacion", identificacion),
                new Claim("email", email),
                new Claim(ClaimTypes.Role, "Cliente")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5),
                AllowRefresh = false
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            _logger.LogInformation("Cliente {Username} autenticado exitosamente.", username);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // LOGOUT
        [HttpGet]
        public async Task<IActionResult> Logout(bool expired = false)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (expired)
            {
                TempData["SessionExpired"] = true;
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExtendSession()
        {
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                User,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(5)
                });

            return Ok(new { success = true, message = "Sesión extendida" });
        }

        #region Helpers de Seguridad (PTL1)

        private bool IsUserLockedOut(string username)
        {
            if (LoginAttempts.TryGetValue(username, out var data))
            {
                if (data.LockoutEnd.HasValue && data.LockoutEnd > DateTime.Now)
                    return true;
                if (data.LockoutEnd.HasValue && data.LockoutEnd <= DateTime.Now)
                {
                    ClearAttempts(username);
                    return false;
                }
            }
            return false;
        }

        private void RegisterFailedAttempt(string username)
        {
            if (!LoginAttempts.ContainsKey(username))
                LoginAttempts[username] = (0, null);
            var current = LoginAttempts[username];
            LoginAttempts[username] = (current.Attempts + 1, current.LockoutEnd);
        }

        private int GetFailedAttempts(string username) =>
            LoginAttempts.TryGetValue(username, out var data) ? data.Attempts : 0;

        private void LockUser(string username) =>
            LoginAttempts[username] = (3, DateTime.Now.AddMinutes(15));

        private void ClearAttempts(string username) =>
            LoginAttempts.Remove(username);

        #endregion
    }
}