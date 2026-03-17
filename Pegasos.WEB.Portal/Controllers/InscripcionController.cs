using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.WEB.Portal.Models.InputModels;
using Pegasos.WEB.Portal.Models.ViewModels;
using Pegasos.WEB.Portal.Services;
using System.Security.Claims;

namespace Pegasos.WEB.Portal.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class InscripcionController : Controller
    {
        private readonly IPagosService _pagosService;
        private readonly ICoreClienteService _coreClienteService;
        private readonly ILogger<InscripcionController> _logger;

        public InscripcionController(
            IPagosService pagosService,
            ICoreClienteService coreClienteService,
            ILogger<InscripcionController> logger)
        {
            _pagosService = pagosService;
            _coreClienteService = coreClienteService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = User.FindFirst("access_token")?.Value ?? "";
            var model = new InscripcionViewModel();

            // Obtener el nombre del usuario del claim
            model.NombreCompleto = User.FindFirst("nombreCompleto")?.Value ?? "Cliente";

            // Intentar obtener la identificación del usuario (esto dependerá de cómo la guardes)
            // Por ahora, la dejamos vacía para que el usuario la ingrese manualmente
            // Idealmente, deberías tener la identificación en algún lado (BD, claim, etc.)

            _logger.LogInformation("Mostrando formulario de inscripción para usuario: {User}",
                User.Identity?.Name);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribir(InscripcionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var token = User.FindFirst("access_token")?.Value ?? "";

            _logger.LogInformation("Token obtenido del claim: {TokenPreview}",
                token?.Substring(0, Math.Min(20, token?.Length ?? 0)) + "...");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token vacío, redirigiendo a login");
                return RedirectToAction("Login", "Auth");
            }

            var input = new InscribirInput
            {
                NumeroCuenta = model.NumeroCuenta,
                Identificacion = model.Identificacion,
                Telefono = model.Telefono
            };

            var result = await _pagosService.InscribirAsync(input, token);

            if (result != null && result.Codigo == 0)
            {
                _logger.LogInformation("Inscripción exitosa para teléfono: {Telefono}", model.Telefono);
                TempData["SuccessMessage"] = result.Descripcion;
                return RedirectToAction("Confirmacion", new { telefono = model.Telefono });
            }
            else
            {
                _logger.LogWarning("Inscripción fallida: {Descripcion}", result?.Descripcion);
                ModelState.AddModelError("", result?.Descripcion ?? "Error en la inscripción");
                return View("Index", model);
            }
        }

        [HttpGet]
        public IActionResult Confirmacion(string telefono)
        {
            ViewBag.Telefono = telefono;
            return View();
        }

        [HttpGet]
        public IActionResult Desinscribir()
        {
            var model = new InscripcionViewModel();
            model.NombreCompleto = User.FindFirst("nombreCompleto")?.Value ?? "";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desinscribir(InscripcionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Desinscribir", model);
            }

            var token = User.FindFirst("access_token")?.Value ?? "";
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var input = new DesinscribirInput
            {
                NumeroCuenta = model.NumeroCuenta,
                Identificacion = model.Identificacion,
                Telefono = model.Telefono
            };

            var result = await _pagosService.DesinscribirAsync(input, token);

            if (result != null && result.Codigo == 0)
            {
                _logger.LogInformation("Desinscripción exitosa para teléfono: {Telefono}", model.Telefono);
                TempData["SuccessMessage"] = result.Descripcion;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                _logger.LogWarning("Desinscripción fallida: {Descripcion}", result?.Descripcion);
                ModelState.AddModelError("", result?.Descripcion ?? "Error en la desinscripción");
                return View("Desinscribir", model);
            }
        }
    }
}