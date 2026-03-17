using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.WEB.Portal.Models.InputModels;
using Pegasos.WEB.Portal.Models.ViewModels;
using Pegasos.WEB.Portal.Services;
using System.Security.Claims;

namespace Pegasos.WEB.Portal.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class TransferenciasController : Controller
    {
        private readonly IPagosService _pagosService;
        private readonly IConsultasService _consultasService;
        private readonly ILogger<TransferenciasController> _logger;

        public TransferenciasController(
            IPagosService pagosService,
            IConsultasService consultasService,
            ILogger<TransferenciasController> logger)
        {
            _pagosService = pagosService;
            _consultasService = consultasService;
            _logger = logger;
        }

        // PTL9: Mostrar formulario de transferencia
        [HttpGet]
        public IActionResult Index()
        {
            var model = new TransferirInput();
            model.NombreOrigen = User.FindFirst("nombreCompleto")?.Value ?? "";
            return View(model);
        }

        // PTL9: Procesar transferencia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RealizarTransferencia(TransferirInput model)
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

            // Asegurar que el nombre origen sea el del usuario autenticado
            model.NombreOrigen = User.FindFirst("nombreCompleto")?.Value ?? "";

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
                ModelState.AddModelError("", result?.Descripcion ?? "Error en la transferencia");
                return View("Index", model);
            }
        }

        [HttpGet]
        public IActionResult Comprobante(string id)
        {
            // Aquí se mostraría un comprobante guardado
            ViewBag.ComprobanteId = id;
            return View();
        }
    }
}