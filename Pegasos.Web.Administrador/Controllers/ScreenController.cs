using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class ScreenController : Controller
    {
        private readonly IScreenService _screenService;
        private readonly ILogger<ScreenController> _logger;

        public ScreenController(IScreenService screenService, ILogger<ScreenController> logger)
        {
            _screenService = screenService;
            _logger = logger;
        }

        // GET: Screen
        public async Task<IActionResult> Index(string filtro = "")
        {
            try
            {
                var pantallas = await _screenService.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    pantallas = pantallas.Where(p =>
                        p.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                var viewModel = new PantallaViewModel
                {
                    Pantallas = pantallas,
                    FiltroNombre = filtro ?? ""
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar pantallas");
                TempData["Error"] = "Error al cargar las pantallas";
                return View(new PantallaViewModel());
            }
        }

        // POST: Screen/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] PantallaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Por favor, complete todos los campos obligatorios";
                    return RedirectToAction(nameof(Index));
                }

                await _screenService.CreateAsync(request);
                TempData["Exito"] = "Pantalla creada exitosamente";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pantalla");
                TempData["Error"] = "Error al crear la pantalla";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Screen/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] PantallaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Por favor, complete todos los campos obligatorios";
                    return RedirectToAction(nameof(Index));
                }

                request.ID_Pantalla = id;
                await _screenService.UpdateAsync(id, request);
                TempData["Exito"] = "Pantalla actualizada exitosamente";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar pantalla");
                TempData["Error"] = "Error al actualizar la pantalla";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Screen/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _screenService.DeleteAsync(id);
                if (result)
                {
                    TempData["Exito"] = "Pantalla eliminada exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar la pantalla";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pantalla");
                TempData["Error"] = "Error al eliminar la pantalla";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}