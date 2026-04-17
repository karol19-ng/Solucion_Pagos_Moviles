using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;
using System.ComponentModel.DataAnnotations;

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class BitacoraController : Controller
    {
        private readonly ITransaccionService _transaccionService;
        private readonly ILogger<BitacoraController> _logger;

        public BitacoraController(ITransaccionService transaccionService, ILogger<BitacoraController> logger)
        {
            _transaccionService = transaccionService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new BitacoraViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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


    }
}