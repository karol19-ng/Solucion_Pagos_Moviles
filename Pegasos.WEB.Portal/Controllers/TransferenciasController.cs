using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.WEB.Portal.Models.InputModels;
using Pegasos.WEB.Portal.Models.ViewModels;
using Pegasos.WEB.Portal.Services;
using System.Security.Claims;

namespace Pegasos.WEB.Portal.Controllers
{
    [Authorize(Roles = "Cliente,Administrador")]
    public class TransferenciasController : Controller
    {
        private readonly IPagosService _pagosService;
        private readonly ILogger<TransferenciasController> _logger;

        public TransferenciasController(
            IPagosService pagosService,
            ILogger<TransferenciasController> logger)
        {
            _pagosService = pagosService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new TransferirInput();

            if (User.IsInRole("Cliente"))
            {
                model.NombreOrigen = User.FindFirst("nombreCompleto")?.Value ?? "Cliente";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RealizarTransferencia(TransferirInput model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorStatusCode"] = "400";
                TempData["ErrorMessage"] = "Datos incompletos. Verifique que todos los campos estén correctamente llenados.";
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
                model.NombreOrigen = User.FindFirst("nombreCompleto")?.Value ?? "Cliente";
            }

            var result = await _pagosService.RealizarTransferenciaAsync(model, token);

            if (result != null && result.Codigo == 0)
            {
                var viewModel = new TransferenciaViewModel
                {
                    TelefonoOrigen = model.TelefonoOrigen,
                    NombreOrigen = model.NombreOrigen,
                    TelefonoDestino = model.TelefonoDestino,
                    Monto = model.Monto,
                    Descripcion = model.Descripcion,
                    Fecha = DateTime.Now,
                    Comprobante = result.Comprobante ?? $"TRX-{DateTime.Now:yyyyMMddHHmmss}"
                };
                return View("Confirmacion", viewModel);
            }
            else
            {
                var errorMsg = result?.Descripcion ?? "Error al procesar la transferencia";
                TempData["ErrorStatusCode"] = result?.Codigo == -1 ? "400" : "500";
                TempData["ErrorMessage"] = errorMsg;
                return View("Index", model);
            }
        }

        [HttpGet]
        public IActionResult Comprobante(string id)
        {
            ViewBag.ComprobanteId = id;
            return View();
        }
    }
}