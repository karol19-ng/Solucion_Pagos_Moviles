using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using System.Security.Claims; // Necesario para ClaimTypes

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Verificación de seguridad básica
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            // SA2: Preparar datos para la vista de bienvenida
            // Nota: Cambié a DashboardViewModel (singular)
            var model = new DashboardViewModels
            {
                NombreCompleto = User.FindFirst("nombreCompleto")?.Value ?? "Usuario Pegasos",
                Email = User.Identity.Name ?? "Sin Email",
                // Buscamos el rol de forma segura
                Rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "Administrador",
                FechaIngreso = DateTime.Now,
                HoraIngreso = DateTime.Now.ToString("HH:mm")
            };

            // SA2: Mensajes dinámicos para la interfaz
            ViewData["WelcomeTitle"] = $"¡Bienvenido, {model.NombreCompleto}!";
            ViewData["PageTitle"] = "Panel de Control";

            return View(model);
        }
    }
}