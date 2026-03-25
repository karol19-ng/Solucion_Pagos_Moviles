using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class EntidadesController : Controller
    {
        private readonly IEntidadService _entidadService;
        private readonly ILogger<EntidadesController> _logger;

        public EntidadesController(
            IEntidadService entidadService,
            ILogger<EntidadesController> logger)
        {
            _entidadService = entidadService;
            _logger = logger;
        }

        // GET: Entidades - Listar todos con paginación y buscador
        public async Task<IActionResult> Index(string searchId = "", int pagina = 1)
        {
            try
            {
                var todos = await _entidadService.ListarTodosAsync() ?? new List<EntidadViewModel>();

                // Filtrar por identificador si se proporciona
                if (!string.IsNullOrWhiteSpace(searchId))
                {
                    todos = todos.Where(e => e.Identificador.Contains(searchId, StringComparison.OrdinalIgnoreCase)).ToList();
                    ViewBag.SearchId = searchId;
                }

                const int registrosPorPagina = 10;
                var totalRegistros = todos.Count;
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

                var entidades = todos
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToList();

                ViewBag.PaginaActual = pagina;
                ViewBag.TotalPaginas = totalPaginas;
                ViewBag.TotalRegistros = totalRegistros;

                return View(entidades);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Sesión expirada. Por favor inicie sesión nuevamente.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index de entidades");
                TempData["Error"] = "Error al cargar las entidades";
                return View(new List<EntidadViewModel>());
            }
        }

        // GET: Entidades/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Entidades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearEntidadViewModel model)
        {
            try
            {
                _logger.LogInformation("Intentando crear entidad - Identificador: {Identificador}, Nombre: {Nombre}",
                    model?.Identificador, model?.Nombre);

                if (model == null)
                {
                    ModelState.AddModelError(string.Empty, "Datos no válidos");
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.Identificador))
                {
                    ModelState.AddModelError("Identificador", "El identificador es requerido");
                }

                if (string.IsNullOrWhiteSpace(model.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El nombre es requerido");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var (exito, mensaje, entidadId) = await _entidadService.CrearAsync(model);
                if (exito)
                {
                    _logger.LogInformation("Entidad creada exitosamente. ID: {EntidadId}", entidadId);
                    TempData["Success"] = mensaje;
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("No se pudo crear la entidad: {Mensaje}", mensaje);
                TempData["Error"] = mensaje;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear entidad");
                TempData["Error"] = "Error al crear la entidad: " + ex.Message;
            }

            return View(model);
        }

        // GET: Entidades/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var entidad = await _entidadService.ObtenerPorIdAsync(id);
                if (entidad == null)
                {
                    TempData["Error"] = "Entidad no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                var model = new EditarEntidadViewModel
                {
                    Id = entidad.Id,
                    Identificador = entidad.Identificador,
                    Nombre = entidad.Nombre,
                    EstadoId = entidad.EstadoId ?? 1
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar entidad para editar {Id}", id);
                TempData["Error"] = "Error al cargar la entidad";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Entidades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditarEntidadViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Validaciones manuales
            if (string.IsNullOrWhiteSpace(model.Identificador))
            {
                ModelState.AddModelError("Identificador", "El identificador es requerido");
            }

            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es requerido");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var resultado = await _entidadService.ActualizarAsync(model);
                if (resultado)
                {
                    TempData["Success"] = "Entidad actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "No se pudo actualizar la entidad");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar entidad {Id}", id);
                ModelState.AddModelError(string.Empty, "Error al actualizar la entidad");
            }

            return View(model);
        }

        // POST: Entidades/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("=== INTENTANDO ELIMINAR ENTIDAD ===");
                _logger.LogInformation("ID recibido: {Id}", id);

                var resultado = await _entidadService.EliminarAsync(id);

                if (resultado)
                {
                    _logger.LogInformation("Entidad {Id} eliminada exitosamente", id);
                    return Json(new { success = true, message = "Entidad eliminada exitosamente" });
                }
                else
                {
                    _logger.LogWarning("No se pudo eliminar la entidad {Id}", id);
                    return Json(new { success = false, message = "No se pudo eliminar la entidad" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar entidad {Id}", id);
                return Json(new { success = false, message = "Error al eliminar la entidad: " + ex.Message });
            }
        }

        // GET: Entidades/Buscar - Búsqueda en tiempo real
        [HttpGet]
        public async Task<IActionResult> Buscar(string termino)
        {
            try
            {
                var resultados = await _entidadService.BuscarAsync(termino);
                return Json(resultados ?? new List<EntidadViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de entidades");
                return Json(new List<EntidadViewModel>());
            }
        }
    }
}