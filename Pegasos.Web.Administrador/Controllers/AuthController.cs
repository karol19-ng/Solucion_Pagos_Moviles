using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;

namespace Pegasos.Web.Administrador.Controllers
{

        public class AuthController : Controller
        {
            private readonly IAuthService _authService;
            private readonly ILogger<AuthController> _logger;

            private static readonly Dictionary<string, (int Attempts, DateTime? LockoutEnd)> LoginAttempts = new();

            public AuthController(IAuthService authService, ILogger<AuthController> logger)
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

                if (TempData["SessionExpired"] != null) ViewBag.SessionExpired = true;

                ViewData["ReturnUrl"] = returnUrl;
                return View(new LoginViewModel());
            }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model, string? returnUrl = null)
        {
            // if (!ModelState.IsValid) return View(model);

            var username = model.Username;

            // SA1: Verificación de Bloqueo
            if (IsUserLockedOut(username))
            {
               if (!ModelState.IsValid) return View(model);

                var username = model.Username;

                // SA1: Verificación de Bloqueo
                if (IsUserLockedOut(username))
                {
                    model.ErrorMessage = "Cuenta bloqueada temporalmente por seguridad (3 intentos fallidos).";
                    return View(model);
                }

                // LLAMADA AL GATEWAY (Puerto 5001)
                // Aquí el result ya trae el Token firmado con tu clave: 
                // "Pegasos_BancoPegasos_SecretKey_2026_CUC_ProgV_Avance2"
                var result = await _authService.LoginAsync(model.Username, model.Password);

            if (result == null)
            {
                _logger.LogWarning("Login fallido para usuario {Username}: AuthService retornó null", username);

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

            System.Diagnostics.Debug.WriteLine($"DEBUG: Token recibido -> {result.AccessToken}");

            // LOGIN EXITOSO
            ClearAttempts(username);

            var tokenSeguro = result.AccessToken;

            if (string.IsNullOrEmpty(tokenSeguro))
            {
                _logger.LogWarning("¡OJO! El servicio de Auth no devolvió un AccessToken.");
                tokenSeguro = "TOKEN_NO_RECIBIDO";
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username),
    
                new Claim(ClaimTypes.NameIdentifier, result.UsuarioId != 0 ? result.UsuarioId.ToString() : "0"),    
                new Claim("nombreCompleto", result.NombreCompleto ?? model.Username),
                new Claim("access_token", tokenSeguro),
                new Claim("refresh_token", result.RefreshToken ?? ""),
                new Claim("aud", "PegasosBankAPI"), 
                new Claim("iss", "PegasosBankIssuer")
            };

            // Rol fijo o dinámico
            claims.Add(new Claim(ClaimTypes.Role, "Administrador"));



            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    // SA4: Sesión de 5 minutos según Program.cs
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5),
                    AllowRefresh = false
                };

                authProperties.StoreTokens(new[]
                {
                    new Microsoft.AspNetCore.Authentication.AuthenticationToken {
                        Name = "access_token",
                        Value = tokenSeguro
                    }
                });

            await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties);

                _logger.LogInformation("Usuario {Username} autenticado exitosamente vía Gateway.", username);

                return RedirectToAction("Index", "Home");
                model.ErrorMessage = "Cuenta bloqueada temporalmente por seguridad (3 intentos fallidos).";
                return View(model);
            }

            // LLAMADA AL GATEWAY
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

            // 🔴 VERIFICAR QUE EL TOKEN NO SEA NULL O VACÍO
            if (string.IsNullOrEmpty(result.access_token))
            {
                _logger.LogError("¡ERROR CRÍTICO! El token de acceso es null o vacío");
                model.ErrorMessage = "Error de autenticación: no se recibió token válido";
                return View(model);
            }

            // LOGIN EXITOSO
            ClearAttempts(username);

            // Guardar token en sesión como respaldo
            try
            {
                HttpContext.Session.SetString("AccessToken", result.access_token);
                HttpContext.Session.SetString("RefreshToken", result.refresh_token ?? "");
                HttpContext.Session.SetString("UsuarioId", result.usuarioID.ToString());
                HttpContext.Session.SetString("NombreCompleto", result.NombreCompleto ?? "");

                _logger.LogInformation("Token guardado en sesión. Longitud: {Length}", result.access_token.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar token en sesión");
            }

            // Creamos los Claims con el token que nos dio la API
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, model.Username),
    new Claim(ClaimTypes.NameIdentifier, result.usuarioID.ToString()),
    new Claim("nombreCompleto", result.NombreCompleto ?? "Usuario Pegasos"),
    new Claim("access_token", result.access_token),
    new Claim("refresh_token", result.refresh_token ?? "")
};

            claims.Add(new Claim(ClaimTypes.Role, "Administrador"));

            // 🔴 VERIFICAR QUE EL CLAIM TIENE VALOR
            var tokenClaim = claims.FirstOrDefault(c => c.Type == "access_token");
            _logger.LogInformation("Claim access_token creado con valor: {Valor}",
                string.IsNullOrEmpty(tokenClaim?.Value) ? "VACÍO" : $"OK ({tokenClaim.Value.Length} chars)");

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

            _logger.LogInformation("Usuario {Username} autenticado exitosamente vía Gateway.", username);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Logout(bool expired = false)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (expired) TempData["SessionExpired"] = true;
                return RedirectToAction("Login");
            }

            [HttpPost]
            [Authorize]
            public async Task<IActionResult> ExtendSession()
            {
            // SA4: Extender la cookie local de acceso
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null) return Unauthorized();

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5) // Le damos 5 min más
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties);

            return Ok(new { success = true, newTime = 300 });
        }

        [HttpGet]
            public async Task<IActionResult> Logout()
            {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

            #region Helpers de Seguridad (SA1)
            private bool IsUserLockedOut(string username)
            {
                if (LoginAttempts.TryGetValue(username, out var data))
                {
                    if (data.LockoutEnd.HasValue && data.LockoutEnd > DateTime.Now) return true;
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
                if (!LoginAttempts.ContainsKey(username)) LoginAttempts[username] = (0, null);
                var current = LoginAttempts[username];
                LoginAttempts[username] = (current.Attempts + 1, current.LockoutEnd);
            }

            private int GetFailedAttempts(string username) => LoginAttempts.TryGetValue(username, out var data) ? data.Attempts : 0;

            private void LockUser(string username) => LoginAttempts[username] = (3, DateTime.Now.AddMinutes(15));

            private void ClearAttempts(string username) => LoginAttempts.Remove(username);
            #endregion
        }
    
}