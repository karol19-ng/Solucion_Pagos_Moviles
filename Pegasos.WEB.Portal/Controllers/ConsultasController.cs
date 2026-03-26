using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.WEB.Portal.Models.ViewModels;
using Pegasos.WEB.Portal.Services;
using System.Security.Claims;

namespace Pegasos.WEB.Portal.Controllers
{
    [Authorize(Roles = "Cliente")]
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
                return View("Saldo", model);
            }

            // Obtener token e identificación del usuario autenticado
            var token = User.FindFirst("access_token")?.Value ?? "";
            var identificacion = User.FindFirst("identificacion")?.Value ?? "";

            _logger.LogInformation("=== INICIANDO CONSULTA DE SALDO ===");
            _logger.LogInformation("Usuario: {User}", User.Identity?.Name);
            _logger.LogInformation("Claims disponibles:");

            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }

            _logger.LogInformation("Identificación encontrada: '{Identificacion}'", identificacion);
            _logger.LogInformation("Token presente: {TokenPresent}", !string.IsNullOrEmpty(token));

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token no encontrado, redirigiendo a login");
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(identificacion))
            {
                _logger.LogError("IDENTIFICACIÓN NO ENCONTRADA EN CLAIMS");
                ModelState.AddModelError("", "Error de autenticación: No se pudo obtener la identificación del usuario");
                return View("Saldo", model);
            }

            var result = await _consultasService.ConsultarSaldoAsync(model.Telefono, identificacion, token);

            if (result != null)
            {
                _logger.LogInformation("Consulta exitosa para teléfono: {Telefono}", model.Telefono);
                TempData["SuccessMessage"] = "Saldo consultado exitosamente";
                return View("ResultadoSaldo", result);
            }
            else
            {
                _logger.LogWarning("Consulta fallida para teléfono: {Telefono}", model.Telefono);
                ModelState.AddModelError("", "No se pudo consultar el saldo. Verifique que el teléfono esté inscrito en pagos móviles.");
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