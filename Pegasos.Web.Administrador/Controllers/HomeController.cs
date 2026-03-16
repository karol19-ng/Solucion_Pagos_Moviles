using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // SA2: Preparar datos para la vista de bienvenida
            var model = new DashboardViewModels
            {
                NombreCompleto = User.FindFirst("nombreCompleto")?.Value ?? "Usuario",
                Email = User.Identity?.Name ?? "",
                Rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Sin rol",
                FechaIngreso = DateTime.Now,
                HoraIngreso = DateTime.Now.ToString("HH:mm")
            };

            // SA2: Mensaje de bienvenida dinámico
            ViewData["WelcomeTitle"] = $"¡Bienvenido, {model.NombreCompleto}!";
            ViewData["PageTitle"] = "Inicio";

            return View(model);
        }
    }
}
