using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.WEB.Portal.Models;
using Pegasos.WEB.Portal.Models.ViewModels;

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
            // PTL2: Página de bienvenida
            var model = new DashboardViewModel
            {
                NombreCompleto = User.FindFirst("nombreCompleto")?.Value ?? "Cliente",
                Email = User.Identity?.Name ?? "",
                Rol = "Cliente",
                FechaIngreso = DateTime.Now,
                HoraIngreso = DateTime.Now.ToString("HH:mm"),
                CuentasAsociadas = new List<CuentaAsociadaViewModel>
                {
                    // Estos datos vendrían del servicio de consultas
                    new CuentaAsociadaViewModel
                    {
                        NumeroCuenta = "CR123456789",
                        Telefono = "8888-5555",
                        Saldo = 150000,
                        Activa = true
                    }
                }
            };

            ViewData["WelcomeTitle"] = $"¡Bienvenido, {model.NombreCompleto}!";
            ViewData["PageTitle"] = "Inicio - Portal Cliente";

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