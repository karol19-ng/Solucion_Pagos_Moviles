using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.WEB.Portal.Models;
using Pegasos.WEB.Portal.Models.ViewModels;
using System.Security.Claims;

namespace Pegasos.WEB.Portal.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Obtener el nombre completo del usuario desde los claims
            var nombreCompleto = User.FindFirst("nombreCompleto")?.Value ??
                                 User.FindFirst(ClaimTypes.Name)?.Value ??
                                 "Cliente";

            var model = new BienvenidaViewModel
            {
                NombreCompleto = nombreCompleto,
                FechaIngreso = DateTime.Now,
                HoraIngreso = DateTime.Now.ToString("HH:mm")
            };

            _logger.LogInformation("Página de bienvenida mostrada para: {Nombre}", nombreCompleto);
            ViewData["PageTitle"] = "Bienvenido - Portal Cliente";

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}