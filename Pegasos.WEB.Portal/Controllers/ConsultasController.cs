using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.WEB.Portal.Models.ViewModels;
using Pegasos.WEB.Portal.Services;
using System.Security.Claims;

namespace Pegasos.WEB.Portal.Controllers
{
    [Authorize(Roles = "Cliente,Administrador")]
    public class ConsultasController : Controller
    {
        private readonly IConsultasService _consultasService;
        private readonly ILogger<ConsultasController> _logger;

        public ConsultasController(IConsultasService consultasService, ILogger<ConsultasController> logger)
        {
            _consultasService = consultasService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Saldo()
        {
            return View(new SaldoViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsultarSaldo(SaldoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorStatusCode"] = "400";
                TempData["ErrorMessage"] = "Datos incompletos. Por favor, complete todos los campos requeridos.";
                return View("Saldo", model);
            }

            var token = User.FindFirst("access_token")?.Value ?? "";

            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorStatusCode"] = "401";
                TempData["ErrorMessage"] = "Sesión no válida. Por favor, inicie sesión nuevamente.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _consultasService.ConsultarSaldoAsync(model.Telefono, model.Identificacion, token);

            if (result != null)
            {
                _logger.LogInformation("Consulta exitosa para teléfono: {Telefono}", model.Telefono);
                TempData["SuccessMessage"] = "Saldo consultado exitosamente";
                return View("ResultadoSaldo", result);
            }
            else
            {
                TempData["ErrorStatusCode"] = "404";
                TempData["ErrorMessage"] = "No se encontró información. Verifique que la identificación y el teléfono sean correctos y que estén inscritos en pagos móviles.";
                return View("Saldo", model);
            }
        }

        [HttpGet]
        public IActionResult Movimientos()
        {
            return View();
        }
    }
}