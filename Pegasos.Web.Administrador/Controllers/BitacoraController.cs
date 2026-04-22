using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class BitacoraController : Controller
    {
        private readonly IBitacoraAdminService _bitacoraService;
        private readonly ILogger<BitacoraController> _logger;

        public BitacoraController(
            IBitacoraAdminService bitacoraService,
            ILogger<BitacoraController> logger)
        {
            _bitacoraService = bitacoraService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new BitacoraViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
    }
}