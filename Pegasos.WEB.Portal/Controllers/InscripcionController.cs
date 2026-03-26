using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.WEB.Portal.Models.InputModels;
using Pegasos.WEB.Portal.Models.ViewModels;
using Pegasos.WEB.Portal.Services;
using System.Security.Claims;

namespace Pegasos.WEB.Portal.Controllers
{
    [Authorize(Roles = "Cliente,Administrador")]
    public class InscripcionController : Controller
    {
        private readonly IPagosService _pagosService;
        private readonly ILogger<InscripcionController> _logger;

        public InscripcionController(
            IPagosService pagosService,
            ILogger<InscripcionController> logger)
        {
            _pagosService = pagosService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new InscripcionViewModel();

            if (User.IsInRole("Cliente"))
            {
                model.NombreCompleto = User.FindFirst("nombreCompleto")?.Value ?? "Cliente";
                model.Identificacion = User.FindFirst("identificacion")?.Value ?? "";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribir(InscripcionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorStatusCode"] = "400";
                TempData["ErrorMessage"] = "Datos incompletos. Por favor, complete todos los campos.";
                return View("Index", model);
            }

            var token = User.FindFirst("access_token")?.Value ?? "";

            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorStatusCode"] = "401";
                TempData["ErrorMessage"] = "Sesión no válida. Por favor, inicie sesión nuevamente.";
                return RedirectToAction("Login", "Auth");
            }

            if (User.IsInRole("Cliente"))
            {
                var userIdentificacion = User.FindFirst("identificacion")?.Value ?? "";
                if (!string.IsNullOrEmpty(userIdentificacion) && string.IsNullOrEmpty(model.Identificacion))
                {
                    model.Identificacion = userIdentificacion;
                }
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
                var errorMsg = result?.Descripcion ?? "Error en la inscripción";
                TempData["ErrorStatusCode"] = result?.Codigo == -1 ? "400" : "500";
                TempData["ErrorMessage"] = errorMsg;

                if (User.IsInRole("Cliente"))
                {
                    model.NombreCompleto = User.FindFirst("nombreCompleto")?.Value ?? "Cliente";
                }

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

            if (User.IsInRole("Cliente"))
            {
                model.NombreCompleto = User.FindFirst("nombreCompleto")?.Value ?? "Cliente";
                model.Identificacion = User.FindFirst("identificacion")?.Value ?? "";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desinscribir(InscripcionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorStatusCode"] = "400";
                TempData["ErrorMessage"] = "Datos incompletos. Por favor, complete todos los campos.";
                return View("Desinscribir", model);
            }

            var token = User.FindFirst("access_token")?.Value ?? "";
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorStatusCode"] = "401";
                TempData["ErrorMessage"] = "Sesión no válida. Por favor, inicie sesión nuevamente.";
                return RedirectToAction("Login", "Auth");
            }

            if (User.IsInRole("Cliente"))
            {
                var userIdentificacion = User.FindFirst("identificacion")?.Value ?? "";
                if (!string.IsNullOrEmpty(userIdentificacion) && string.IsNullOrEmpty(model.Identificacion))
                {
                    model.Identificacion = userIdentificacion;
                }
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
                var errorMsg = result?.Descripcion ?? "Error en la desinscripción";
                TempData["ErrorStatusCode"] = result?.Codigo == -1 ? "400" : "500";
                TempData["ErrorMessage"] = errorMsg;

                if (User.IsInRole("Cliente"))
                {
                    model.NombreCompleto = User.FindFirst("nombreCompleto")?.Value ?? "Cliente";
                }

                return View("Desinscribir", model);
            }
        }
    }
}