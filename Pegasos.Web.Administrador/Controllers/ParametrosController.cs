using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class ParametrosController : Controller
    {
        private readonly IParametroService _parametroService;
        private readonly ILogger<ParametrosController> _logger;

        public ParametrosController(
            IParametroService parametroService,
            ILogger<ParametrosController> logger)
        {
            _parametroService = parametroService;
            _logger = logger;
        }

        // GET: Parametros - Listar todos con paginación y buscador
        public async Task<IActionResult> Index(string searchId = "", int pagina = 1)
        {
            try
            {
                var todos = await _parametroService.ListarTodosAsync() ?? new List<ParametroViewModel>();

                // Filtrar por ID si se proporciona
                if (!string.IsNullOrWhiteSpace(searchId))
                {
                    todos = todos.Where(p => p.Id.Contains(searchId, StringComparison.OrdinalIgnoreCase)).ToList();
                    ViewBag.SearchId = searchId;
                }

                const int registrosPorPagina = 10;
                var totalRegistros = todos.Count;
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

                var parametros = todos
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToList();

                ViewBag.PaginaActual = pagina;
                ViewBag.TotalPaginas = totalPaginas;
                ViewBag.TotalRegistros = totalRegistros;

                return View(parametros);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Sesión expirada. Por favor inicie sesión nuevamente.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index de parámetros");
                TempData["Error"] = "Error al cargar los parámetros";
                return View(new List<ParametroViewModel>());
            }
        }

        // GET: Parametros/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Parametros/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearParametroViewModel model)
        {
            try
            {
                _logger.LogInformation("Intentando crear parámetro - ID: {Id}, Valor: {Valor}",
                    model?.Id, model?.Valor);

                if (model == null)
                {
                    ModelState.AddModelError(string.Empty, "Datos no válidos");
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    ModelState.AddModelError("Id", "El ID del parámetro es requerido");
                }

                if (string.IsNullOrWhiteSpace(model.Valor))
                {
                    ModelState.AddModelError("Valor", "El valor es requerido");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var (exito, mensaje) = await _parametroService.CrearAsync(model);
                if (exito)
                {
                    _logger.LogInformation("Parámetro creado exitosamente. ID: {Id}", model.Id);
                    TempData["Success"] = mensaje;
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("No se pudo crear el parámetro: {Mensaje}", mensaje);
                TempData["Error"] = mensaje;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear parámetro");
                TempData["Error"] = "Error al crear el parámetro: " + ex.Message;
            }

            return View(model);
        }

        // GET: Parametros/Edit/{id}
        [HttpGet("Parametros/Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var parametro = await _parametroService.ObtenerPorIdAsync(id);
                if (parametro == null)
                {
                    TempData["Error"] = "Parámetro no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var model = new EditarParametroViewModel
                {
                    Id = parametro.Id,
                    Valor = parametro.Valor,
                    EstadoId = parametro.EstadoId ?? 1
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar parámetro para editar {Id}", id);
                TempData["Error"] = "Error al cargar el parámetro";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Parametros/Edit/{id}
        [HttpPost("Parametros/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditarParametroViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Validaciones manuales
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                ModelState.AddModelError("Id", "El ID del parámetro es requerido");
            }

            if (string.IsNullOrWhiteSpace(model.Valor))
            {
                ModelState.AddModelError("Valor", "El valor es requerido");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var resultado = await _parametroService.ActualizarAsync(model);
                if (resultado)
                {
                    TempData["Success"] = "Parámetro actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "No se pudo actualizar el parámetro");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parámetro {Id}", id);
                ModelState.AddModelError(string.Empty, "Error al actualizar el parámetro");
            }

            return View(model);
        }

        // POST: Parametros/Delete/{id}
        [HttpPost("Parametros/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                _logger.LogInformation("=== INTENTANDO ELIMINAR PARÁMETRO ===");
                _logger.LogInformation("ID recibido: {Id}", id);

                var resultado = await _parametroService.EliminarAsync(id);

                if (resultado)
                {
                    _logger.LogInformation("Parámetro {Id} eliminado exitosamente", id);
                    return Json(new { success = true, message = "✅ Parámetro eliminado exitosamente" });
                }
                else
                {
                    _logger.LogWarning("No se pudo eliminar el parámetro {Id}", id);
                    return Json(new { success = false, message = "❌ No se pudo eliminar el parámetro" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar parámetro {Id}", id);
                return Json(new { success = false, message = "❌ Error al eliminar el parámetro: " + ex.Message });
            }
        }

        // GET: Parametros/Buscar - Búsqueda en tiempo real
        [HttpGet]
        public async Task<IActionResult> Buscar(string termino)
        {
            try
            {
                var resultados = await _parametroService.BuscarAsync(termino);
                return Json(resultados ?? new List<ParametroViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de parámetros");
                return Json(new List<ParametroViewModel>());
            }
        }
    }
}