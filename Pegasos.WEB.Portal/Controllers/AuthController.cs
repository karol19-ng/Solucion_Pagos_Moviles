using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.WEB.Portal.Models.ViewModels;
using Pegasos.WEB.Portal.Services;

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
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            if (TempData["SessionExpired"] != null)
                ViewBag.SessionExpired = true;

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginPortalViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginPortalViewModel model, string? returnUrl = null)
        {
            var username = model.Username;

            // PTL1: Verificar bloqueo por 3 intentos fallidos
            if (IsUserLockedOut(username))
            {
                model.ErrorMessage = "Cuenta bloqueada temporalmente por seguridad (3 intentos fallidos).";
                return View(model);
            }

            var result = await _authService.LoginAsync(model.Username, model.Password);

            if (result == null)
            {
                RegisterFailedAttempt(username);
                int attempts = GetFailedAttempts(username);
                int remaining = 3 - attempts;

                if (remaining <= 0)
                {
                    LockUser(username);
                    model.ErrorMessage = "Usuario bloqueado. Intente en 15 minutos.";
                }
                else
                {
                    model.ErrorMessage = $"Credenciales incorrectas. Intentos restantes: {remaining}";
                }
                return View(model);
            }

            // Login exitoso
            ClearAttempts(username);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.NameIdentifier, result.UsuarioId.ToString()),
                new Claim("nombreCompleto", result.NombreCompleto ?? "Cliente Pegasos"),
                new Claim("access_token", result.AccessToken),
                new Claim("refresh_token", result.RefreshToken ?? ""),
                new Claim(ClaimTypes.Role, "Cliente") // Rol fijo para portal
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5), // PTL4: 5 minutos
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(bool expired = false)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (expired)
                TempData["SessionExpired"] = true;
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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

        #region Helpers de Seguridad
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