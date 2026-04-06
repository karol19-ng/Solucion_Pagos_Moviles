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
                _logger.LogInformation("=== RECIBIDA PETICIÓN CREATE DESDE VISTA ===");
                _logger.LogInformation("Modelo recibido - Nombre: {Nombre}, Descripción: {Descripcion}, Ruta: {Ruta}",
                    model?.Nombre, model?.Descripcion, model?.Ruta);

                if (model == null)
                {
                    _logger.LogWarning("Modelo es null");
                    ModelState.AddModelError(string.Empty, "Datos no válidos");
                    return View(model);
                }

                // Validaciones 
                if (string.IsNullOrWhiteSpace(model.Nombre))
                {
                    _logger.LogWarning("Nombre vacío");
                    ModelState.AddModelError("Nombre", "El nombre es requerido");
                }
                else
                {
                    _logger.LogInformation("Nombre válido: {Nombre}", model.Nombre);
                }

                if (string.IsNullOrWhiteSpace(model.Descripcion))
                {
                    _logger.LogWarning("Descripción vacía");
                    ModelState.AddModelError("Descripcion", "La descripción es requerida");
                }
                else
                {
                    _logger.LogInformation("Descripción válida: {Descripcion}", model.Descripcion);
                }

                if (string.IsNullOrWhiteSpace(model.Ruta))
                {
                    _logger.LogWarning("Ruta vacía");
                    ModelState.AddModelError("Ruta", "La ruta es requerida");
                }
                else
                {
                    _logger.LogInformation("Ruta válida: {Ruta}", model.Ruta);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido - Errores: {ErrorCount}", ModelState.ErrorCount);
                    return View(model);
                }

                _logger.LogInformation("Llamando a servicio para crear pantalla...");
                var resultado = await _pantallaService.CrearAsync(model);

                _logger.LogInformation("Resultado del servicio: {Resultado}", resultado);

                if (resultado)
                {
                    _logger.LogInformation("Pantalla creada exitosamente");
                    TempData["Success"] = "Pantalla creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("Servicio devolvió false");
                ModelState.AddModelError(string.Empty, "No se pudo crear la pantalla. Verifique los datos.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pantalla");
                ModelState.AddModelError(string.Empty, "Error al crear la pantalla: " + ex.Message);
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
            _logger.LogInformation("=== PROCESAR EDICIÓN DE PANTALLA ===");
            _logger.LogInformation("ID recibido: {Id}", id);
            _logger.LogInformation("Modelo - Id: {ModelId}, Nombre: {Nombre}, Descripción: {Descripcion}, Ruta: {Ruta}, Estado: {Estado}",
                model?.Id, model?.Nombre, model?.Descripcion, model?.Ruta, model?.Estado);

            if (model == null)
            {
                _logger.LogWarning("Modelo es null");
                return NotFound();
            }

            if (id != model.Id)
            {
                _logger.LogWarning("El ID de la URL ({UrlId}) no coincide con el ID del modelo ({ModelId})", id, model.Id);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Errores: {Errors}", string.Join(", ", errors));
                return View(model);
            }

            try
            {
                _logger.LogInformation("Llamando a servicio para actualizar pantalla...");
                var resultado = await _pantallaService.ActualizarAsync(model);

                _logger.LogInformation("Resultado del servicio: {Resultado}", resultado);

                if (resultado)
                {
                    _logger.LogInformation("Pantalla {Id} actualizada exitosamente", id);
                    TempData["Success"] = "Pantalla actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("No se pudo actualizar la pantalla {Id}", id);
                ModelState.AddModelError(string.Empty, "No se pudo actualizar la pantalla");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar pantalla {Id}", id);
                ModelState.AddModelError(string.Empty, "Error al actualizar la pantalla: " + ex.Message);
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
                _logger.LogInformation("=== DELETE CONTROLLER ===");
                _logger.LogInformation("ID recibido desde la vista: {Id}", id);

                var resultado = await _pantallaService.EliminarAsync(id);

                if (resultado)
                {
                    _logger.LogInformation("Pantalla {Id} eliminada exitosamente", id);
                    return Json(new { success = true, message = "Pantalla eliminada exitosamente" });
                }
                else
                {
                    _logger.LogWarning("No se pudo eliminar la pantalla {Id}", id);
                    return Json(new { success = false, message = "No se pudo eliminar la pantalla" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pantalla {Id}", id);
                return Json(new { success = false, message = ex.Message });
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