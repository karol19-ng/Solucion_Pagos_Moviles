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
        private readonly ILogger<InscripcionController> _logger;

        public InscripcionController(IPagosService pagosService, ILogger<InscripcionController> logger)
        {
            _pagosService = pagosService;
            _logger = logger;
        }

        // PTL5: Mostrar formulario de inscripción
        [HttpGet]
        public IActionResult Index()
        {
            var model = new InscripcionViewModel();
            // Aquí se precargaría la información del core bancario
            // Por ahora, usamos datos de ejemplo
            model.NombreCompleto = User.FindFirst("nombreCompleto")?.Value ?? "";
            model.Identificacion = ""; // Vendría del core

            return View(model);
        }

        // PTL5: Procesar inscripción
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribir(InscripcionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var token = User.FindFirst("access_token")?.Value ?? "";
            if (string.IsNullOrEmpty(token))
            {
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
                TempData["SuccessMessage"] = result.Descripcion;
                return RedirectToAction("Confirmacion", new { telefono = model.Telefono });
            }
            else
            {
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

        // PTL6: Mostrar formulario de desinscripción
        [HttpGet]
        public IActionResult Desinscribir()
        {
            var model = new InscripcionViewModel();
            return View(model);
        }

        // PTL6: Procesar desinscripción
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
                TempData["SuccessMessage"] = result.Descripcion;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", result?.Descripcion ?? "Error en la desinscripción");
                return View("Desinscribir", model);
            }
        }
    }
}