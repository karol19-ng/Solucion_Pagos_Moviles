using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class PantallasController : Controller
    {
        private readonly IPantallaService _pantallaService;
        private readonly ILogger<PantallasController> _logger;

        public PantallasController(
            IPantallaService pantallaService,
            ILogger<PantallasController> logger)
        {
            _pantallaService = pantallaService;
            _logger = logger;
        }

        // GET: Pantallas
        public async Task<IActionResult> Index(int pagina = 1)
        {
            try
            {
                var todos = await _pantallaService.ListarTodosAsync() ?? new List<PantallaViewModel>();

                const int registrosPorPagina = 10;
                var totalRegistros = todos.Count;
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

                var pantallas = todos
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToList();

                ViewBag.PaginaActual = pagina;
                ViewBag.TotalPaginas = totalPaginas;
                ViewBag.TotalRegistros = totalRegistros;

                return View(pantallas);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Sesión expirada. Por favor inicie sesión nuevamente.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index de pantallas");
                TempData["Error"] = "Error al cargar las pantallas";
                return View(new List<PantallaViewModel>());
            }
        }

        // GET: Pantallas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pantallas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearPantallaViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var resultado = await _pantallaService.CrearAsync(model);
                if (resultado)
                {
                    TempData["Success"] = "Pantalla creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "No se pudo crear la pantalla");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pantalla");
                ModelState.AddModelError(string.Empty, "Error al crear la pantalla");
            }

            return View(model);
        }

        // GET: Pantallas/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var pantalla = await _pantallaService.ObtenerPorIdAsync(id);
                if (pantalla == null)
                {
                    TempData["Error"] = "Pantalla no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                var model = new EditarPantallaViewModel
                {
                    Id = pantalla.Id,
                    Nombre = pantalla.Nombre,
                    Descripcion = pantalla.Descripcion,
                    Ruta = pantalla.Ruta,
                    Estado = pantalla.Estado
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar pantalla para editar {Id}", id);
                TempData["Error"] = "Error al cargar la pantalla";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Pantallas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditarPantallaViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var resultado = await _pantallaService.ActualizarAsync(model);
                if (resultado)
                {
                    TempData["Success"] = "Pantalla actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "No se pudo actualizar la pantalla");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar pantalla {Id}", id);
                ModelState.AddModelError(string.Empty, "Error al actualizar la pantalla");
            }

            return View(model);
        }

        // POST: Pantallas/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Intentando eliminar pantalla {Id}", id);

                var resultado = await _pantallaService.EliminarAsync(id);

                if (resultado)
                {
                    return Json(new { success = true, message = "Pantalla eliminada exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo eliminar la pantalla" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pantalla {Id}", id);
                return Json(new { success = false, message = "Error al eliminar la pantalla" });
            }
        }

        // GET: Pantallas/Buscar
        [HttpGet]
        public async Task<IActionResult> Buscar(string termino)
        {
            try
            {
                var todos = await _pantallaService.ListarTodosAsync() ?? new List<PantallaViewModel>();

                if (!string.IsNullOrWhiteSpace(termino))
                {
                    termino = termino.ToLower();
                    todos = todos.Where(p =>
                        (p.Nombre?.ToLower()?.Contains(termino) ?? false)
                    ).ToList();
                }

                return Json(todos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de pantallas");
                return Json(new List<PantallaViewModel>());
            }
        }
    }
}