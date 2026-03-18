using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;


namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _gatewayUrl = "http://localhost:5000/gateway/user";

        public ClientesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // LISTAR USUARIOS
        public async Task<IActionResult> Index(string searchString)
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "access_token");
            var token = claim?.Value;

            // Si el token es nulo o es el string de error que pusimos arriba
            if (string.IsNullOrEmpty(token) || token == "TOKEN_NO_RECIBIDO")
            {
                ViewBag.Error = "No se encontró el Token. Por favor, intenta cerrar sesión e ingresar de nuevo.";
                return View("~/Views/Clientes/Index.cshtml", new List<UsuarioViewModel>());
            }
            // Si no hay token, no te saquemos todavía, mejor veamos el error
            if (string.IsNullOrEmpty(token))
            {
                var totalClaims = User.Claims.Count();
                ViewBag.Error = "No se encontró un token de acceso. Por favor, vuelve a iniciar sesión.";
                return View("~/Views/Clientes/Index.cshtml", new List<UsuarioViewModel>());
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try
            {
                var response = await client.GetAsync(_gatewayUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var usuarios = JsonConvert.DeserializeObject<List<UsuarioViewModel>>(json);
                    // ... lógica de filtrado ...
                    return View("~/Views/Clientes/Index.cshtml", usuarios);
                }

                // Si el API responde pero con error (ej: 404 o 500)
                ViewBag.Error = $"El API respondió con error: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                // Esto nos dirá si es culpa del puerto 5000
                ViewBag.Error = "Error de conexión: " + ex.Message;
            }

            // Si llegamos aquí, algo falló, pero al menos cargamos la vista vacía con el mensaje de error
            return View("~/Views/Clientes/Index.cshtml", new List<UsuarioViewModel>());
        }

        // CREAR USUARIO (POST)
        [HttpPost]
        public async Task<IActionResult> Create(CrearUsuarioViewModel model)
        {
            var client = _httpClientFactory.CreateClient();
            var token = User.FindFirst("access_token")?.Value;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Objeto anónimo con los nombres exactos de tu Swagger
            var apiData = new
            {
                iD_Usuario = 0,
                nombre_Completo = model.NombreCompleto,
                tipo_Identificacion = model.TipoIdentificacion,
                identificacion = model.Identificacion,
                email = model.Email,
                telefono = model.Telefono,
                usuario = model.Email, // Usamos el email como nombre de usuario
                contraseña = model.Password,
                iD_Estado = 1,
                iD_Rol = model.Rol == "Administrador" ? 1 : 2
            };

            var content = new StringContent(JsonConvert.SerializeObject(apiData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_gatewayUrl, content);

            if (response.IsSuccessStatusCode) return RedirectToAction("Index");
            return View(model);
        }

        // ELIMINAR USUARIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id) // Ahora recibe el string de identificación
        {
            var client = _httpClientFactory.CreateClient();
            var token = User.FindFirst("access_token")?.Value;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // El microservicio ahora recibirá: http://localhost:5000/gateway/user/117220456
            var response = await client.DeleteAsync($"{_gatewayUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "No se pudo eliminar el usuario con identificación: " + id;
            return RedirectToAction(nameof(Index));
        }
    }
}