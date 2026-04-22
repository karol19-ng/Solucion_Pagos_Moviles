using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;
<<<<<<< HEAD
<<<<<<< HEAD
=======
using System.ComponentModel.DataAnnotations;
>>>>>>> 87e64982bb773abe1b92ecdc46068eb40859dd1e
=======
using System.ComponentModel.DataAnnotations;
>>>>>>> 87e64982bb773abe1b92ecdc46068eb40859dd1e

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class BitacoraController : Controller
    {
<<<<<<< HEAD
<<<<<<< HEAD
        private readonly IBitacoraAdminService _bitacoraService;
        private readonly ILogger<BitacoraController> _logger;

        public BitacoraController(
            IBitacoraAdminService bitacoraService,
            ILogger<BitacoraController> logger)
        {
            _bitacoraService = bitacoraService;
=======
=======
>>>>>>> 87e64982bb773abe1b92ecdc46068eb40859dd1e
        private readonly ITransaccionService _transaccionService;
        private readonly ILogger<BitacoraController> _logger;

        public BitacoraController(ITransaccionService transaccionService, ILogger<BitacoraController> logger)
        {
            _transaccionService = transaccionService;
<<<<<<< HEAD
>>>>>>> 87e64982bb773abe1b92ecdc46068eb40859dd1e
=======
>>>>>>> 87e64982bb773abe1b92ecdc46068eb40859dd1e
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new BitacoraViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
<<<<<<< HEAD
<<<<<<< HEAD
        public async Task<IActionResult> Index(BitacoraViewModel filtro)
        {
            try
            {
                var resultados = await _bitacoraService.ConsultarTransaccionesAsync(filtro.Fecha);
                filtro.Resultados = resultados;
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Sesión expirada. Por favor inicie sesión nuevamente.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar bitácora");
                ViewBag.Error = "Error de conexión: " + ex.Message;
            }

            return View(filtro);
        }
=======
=======
>>>>>>> 87e64982bb773abe1b92ecdc46068eb40859dd1e
        public async Task<IActionResult> Index(BitacoraViewModel filtro, string Fecha)
        {
            try
            {
                if (!string.IsNullOrEmpty(Fecha) && DateTime.TryParse(Fecha, out var fechaParsed))
                {
                    filtro.Fecha = fechaParsed;
                    var reporte = await _transaccionService.ConsultarTransaccionesPorFechaAsync(fechaParsed);

                    ViewBag.Debug = $"Fecha: {fechaParsed:yyyy-MM-dd} | Transacciones: {reporte?.Transacciones?.Count ?? -1}";

                    filtro.Resultados = reporte?.Transacciones?.Select(t => new BitacoraItemViewModel
                    {
                        Fecha = t.Fecha,
                        TelefonoOrigen = t.TelefonoOrigen,
                        TelefonoDestino = t.TelefonoDestino,
                        Monto = t.Monto
                    }).ToList() ?? new();
                }
                else
                {
                    ViewBag.Debug = $"Fecha recibida: '{Fecha}' — no se pudo parsear";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Debug = $"EXCEPCION: {ex.Message}";
                _logger.LogError(ex, "Error al consultar bitácora");
            }
            return View(filtro);
        }


<<<<<<< HEAD
>>>>>>> 87e64982bb773abe1b92ecdc46068eb40859dd1e
=======
>>>>>>> 87e64982bb773abe1b92ecdc46068eb40859dd1e
    }
}