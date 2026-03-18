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
            model.NombreOrigen = User.FindFirst("nombreCompleto")?.Value ??
                                 User.FindFirst(ClaimTypes.Name)?.Value ??
                                 "Cliente";

            var token = User.FindFirst("access_token")?.Value ?? "";
            _logger.LogInformation("Token en GET Index: {Token}",
                !string.IsNullOrEmpty(token) ? token.Substring(0, Math.Min(20, token.Length)) + "..." : "NO HAY TOKEN");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RealizarTransferencia(TransferirInput model)
        {
            // Obtener el token ANTES de validar el modelo
            var token = User.FindFirst("access_token")?.Value ?? "";

            _logger.LogInformation("=== INICIANDO TRANSFERENCIA ===");
            _logger.LogInformation("Usuario: {User}", User.Identity?.Name);
            _logger.LogInformation("Token presente: {TokenPresent}", !string.IsNullOrEmpty(token));
            _logger.LogInformation("Token: {Token}",
                !string.IsNullOrEmpty(token) ? token.Substring(0, Math.Min(20, token.Length)) + "..." : "NO HAY TOKEN");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No hay token, redirigiendo a login");
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View("Index", model);
            }

            // Asegurar que el nombre origen sea el del usuario autenticado
            model.NombreOrigen = User.FindFirst("nombreCompleto")?.Value ??
                                 User.FindFirst(ClaimTypes.Name)?.Value ??
                                 "Cliente";

            _logger.LogInformation("Enviando transferencia: Origen={Origen}, Destino={Destino}, Monto={Monto}",
                model.TelefonoOrigen, model.TelefonoDestino, model.Monto);

            var result = await _pagosService.RealizarTransferenciaAsync(model, token);

            if (result != null && result.Codigo == 0)
            {
                _logger.LogInformation("Transferencia exitosa. Comprobante: {Comprobante}", result.Comprobante);

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
                var errorMsg = result?.Descripcion ?? "Error en la transferencia";
                _logger.LogWarning("Transferencia fallida: {Error}", errorMsg);
                ModelState.AddModelError("", errorMsg);
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