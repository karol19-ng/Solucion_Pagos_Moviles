

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.WEB.Admin.Models;
using Pegasos.WEB.Admin.Services;
using System.Security.Claims;

namespace Pegasos.WEB.Admin.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly IAdminService _adminService;

        public ReportesController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET /Reportes/Transacciones
        // Muestra el formulario con fecha de hoy por defecto
        [HttpGet]
        public IActionResult Transacciones()
        {
            ViewBag.FechaSeleccionada = DateTime.Today.ToString("yyyy-MM-dd");
            return View(new ReporteTransaccionViewModel());
        }

        // POST /Reportes/Transacciones
        // Recibe la fecha y consulta el reporte
        [HttpPost]
        public async Task<IActionResult> Transacciones(DateTime fecha)
        {
            ViewBag.FechaSeleccionada = fecha.ToString("yyyy-MM-dd");

            if (fecha == default)
            {
                ModelState.AddModelError("", "Debe seleccionar una fecha válida.");
                return View(new ReporteTransaccionViewModel());
            }

            // Obtener token del usuario autenticado
            var token = User.FindFirst("AccessToken")?.Value ?? string.Empty;

            var reporte = await _adminService.ObtenerReporteTransaccionesAsync(fecha, token);

            if (reporte == null)
            {
                ModelState.AddModelError("", "No se pudo obtener el reporte. Verifique su sesión.");
                return View(new ReporteTransaccionViewModel());
            }

            return View(reporte);
        }
    }
}