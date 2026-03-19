using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ClientesController> _logger;
        private readonly string _gatewayUrl = "https://localhost:7096/gateway/user";

        public ClientesController(
            IHttpClientFactory httpClientFactory,
            ILogger<ClientesController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // MÉTODO HELPER PARA CREAR HTTPCLIENT CON TOKEN
        private HttpClient CreateHttpClientWithToken(string token)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(30);

            return client;
        }

        // MÉTODO HELPER PARA OBTENER TOKEN
        private async Task<string> GetAccessTokenAsync()
        {
            // 1. Primero intentar de AuthenticationProperties (lugar correcto)
            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (authResult.Succeeded)
            {
                var token = authResult.Properties.GetTokenValue("access_token");
                if (!string.IsNullOrEmpty(token) && token != "TOKEN_NO_RECIBIDO")
                {
                    _logger.LogInformation("Token obtenido de AuthenticationProperties");
                    return token;
                }
            }

            // 2. Fallback a Claims
            var claimToken = User.FindFirst("access_token")?.Value;
            if (!string.IsNullOrEmpty(claimToken) && claimToken != "TOKEN_NO_RECIBIDO")
            {
                _logger.LogInformation("Token obtenido de Claims (fallback)");
                return claimToken;
            }

            _logger.LogWarning("No se pudo obtener el token");
            return null;
        }

        // LISTAR USUARIOS
        public async Task<IActionResult> Index(string searchString)
        {
            // Obtener token usando el método helper
            var token = await GetAccessTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "No se encontró el Token de autenticación. Por favor, intenta cerrar sesión e ingresar de nuevo.";
                return View("~/Views/Clientes/Index.cshtml", new List<UsuarioViewModel>());
            }
            _logger.LogWarning("========== TOKEN COMPLETO ==========");
            _logger.LogWarning(token);  // Esto mostrará el token completo
            _logger.LogWarning("====================================");

            try
            {
                var client = CreateHttpClientWithToken(token);
                _logger.LogWarning("Header Authorization: {Header}", client.DefaultRequestHeaders.Authorization?.ToString());
                _logger.LogInformation("Enviando petición a Gateway: {Url}", _gatewayUrl);
                _logger.LogInformation("Token: Bearer {Token}", token.Substring(0, Math.Min(30, token.Length)) + "...");

                var response = await client.GetAsync(_gatewayUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Respuesta del Gateway - Status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var usuarios = JsonConvert.DeserializeObject<List<UsuarioViewModel>>(responseContent);

                    // Aplicar filtro si hay searchString
                    if (!string.IsNullOrEmpty(searchString) && usuarios != null)
                    {
                        usuarios = usuarios.Where(u =>
                            (u.NombreCompleto?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (u.Identificacion?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
                        ).ToList();
                        ViewData["CurrentFilter"] = searchString;
                    }

                    return View("~/Views/Clientes/Index.cshtml", usuarios ?? new List<UsuarioViewModel>());
                }
                else
                {
                    _logger.LogWarning("Gateway error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    ViewBag.Error = $"Error del Gateway: {response.StatusCode} - {responseContent}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con el gateway");
                ViewBag.Error = "Error de conexión: " + ex.Message;
            }

            return View("~/Views/Clientes/Index.cshtml", new List<UsuarioViewModel>());
        }

        // CREAR USUARIO (POST)
        [HttpPost]
        public async Task<IActionResult> Create(CrearUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var token = await GetAccessTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", "No se pudo obtener el token de autenticación. Por favor, cierra sesión y vuelve a ingresar.");
                return View(model);
            }

            try
            {
                var client = CreateHttpClientWithToken(token);

                var apiData = new
                {
                    iD_Usuario = 0,
                    nombre_Completo = model.NombreCompleto,
                    tipo_Identificacion = model.TipoIdentificacion,
                    identificacion = model.Identificacion,
                    email = model.Email,
                    telefono = model.Telefono,
                    usuario = model.Email,
                    contraseña = model.Password,
                    iD_Estado = 1,
                    iD_Rol = model.Rol == "Administrador" ? 1 : 2
                };

                var json = JsonConvert.SerializeObject(apiData);
                _logger.LogInformation("Enviando datos a Gateway: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_gatewayUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta Create - Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Usuario creado exitosamente.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", $"Error al crear el usuario: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                ModelState.AddModelError("", "Error de conexión: " + ex.Message);
            }

            return View(model);
        }

        // ELIMINAR USUARIO - CORREGIDO (sin atributo duplicado)
        [HttpPost]
        [ValidateAntiForgeryToken]  // ← Solo UNA vez
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Identificación no válida.";
                return RedirectToAction(nameof(Index));
            }

            var token = await GetAccessTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No se pudo obtener el token de autenticación. Por favor, cierra sesión y vuelve a ingresar.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var client = CreateHttpClientWithToken(token);
                var deleteUrl = $"{_gatewayUrl}/{id}";

                _logger.LogInformation("Eliminando usuario en URL: {Url}", deleteUrl);

                var response = await client.DeleteAsync(deleteUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Respuesta Delete - Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Usuario eliminado exitosamente.";
                }
                else
                {
                    TempData["Error"] = $"No se pudo eliminar el usuario: {response.StatusCode} - {responseContent}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID: {Id}", id);
                TempData["Error"] = "Error de conexión: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}