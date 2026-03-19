using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly IRolService _rolService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            IRolService rolService,
            ILogger<RolesController> logger)
        {
            _rolService = rolService;
            _logger = logger;
        }

        // GET: Roles
        public async Task<IActionResult> Index(int pagina = 1)
        {
            try
            {
                var todos = await _rolService.ListarTodosAsync() ?? new List<RolViewModel>();

                const int registrosPorPagina = 10;
                var totalRegistros = todos.Count;
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

                var roles = todos
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToList();

                ViewBag.PaginaActual = pagina;
                ViewBag.TotalPaginas = totalPaginas;
                ViewBag.TotalRegistros = totalRegistros;

                return View(roles);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Sesión expirada. Por favor inicie sesión nuevamente.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index de roles");
                TempData["Error"] = "Error al cargar los roles";
                return View(new List<RolViewModel>());
            }
        }

        // GET: Roles/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var pantallas = await _rolService.ObtenerPantallasConAsignacionAsync();
                ViewBag.Pantallas = pantallas ?? new List<PantallaAsignadaViewModel>();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de creación");
                TempData["Error"] = "Error al cargar el formulario";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearRolViewModel model)
        {
            try
            {
                _logger.LogInformation("=== RECIBIDA PETICIÓN CREATE ROL ===");
                _logger.LogInformation("Nombre: {Nombre}, Pantallas: {Pantallas}",
                    model?.Nombre, model?.PantallasSeleccionadas != null ? string.Join(",", model.PantallasSeleccionadas) : "ninguna");

                if (model == null)
                {
                    _logger.LogWarning("Modelo es null");
                    ModelState.AddModelError(string.Empty, "Datos no válidos");
                    ViewBag.Pantallas = await _rolService.ObtenerPantallasConAsignacionAsync();
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.Nombre))
                {
                    _logger.LogWarning("Nombre vacío");
                    ModelState.AddModelError("Nombre", "El nombre es requerido");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido");
                    ViewBag.Pantallas = await _rolService.ObtenerPantallasConAsignacionAsync();
                    return View(model);
                }

                var resultado = await _rolService.CrearAsync(model);
                if (resultado)
                {
                    _logger.LogInformation("✅ Rol creado exitosamente");
                    TempData["Success"] = "Rol creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("❌ No se pudo crear el rol");
                ModelState.AddModelError(string.Empty, "No se pudo crear el rol");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear rol");
                ModelState.AddModelError(string.Empty, "Error al crear el rol: " + ex.Message);
            }

            ViewBag.Pantallas = await _rolService.ObtenerPantallasConAsignacionAsync();
            return View(model);
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var rol = await _rolService.ObtenerPorIdAsync(id);
                if (rol == null)
                {
                    TempData["Error"] = "Rol no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var pantallas = await _rolService.ObtenerPantallasConAsignacionAsync(id);

                var model = new EditarRolViewModel
                {
                    Id = rol.Id,
                    Nombre = rol.Nombre,
                    PantallasSeleccionadas = pantallas?
                        .Where(p => p.Asignada)
                        .Select(p => p.Id)
                        .ToList() ?? new List<int>()
                };

                ViewBag.Pantallas = pantallas ?? new List<PantallaAsignadaViewModel>();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar rol para editar {Id}", id);
                TempData["Error"] = "Error al cargar el rol";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditarRolViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            try
            {
                if (string.IsNullOrWhiteSpace(model.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El nombre es requerido");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Pantallas = await _rolService.ObtenerPantallasConAsignacionAsync(id);
                    return View(model);
                }

                var resultado = await _rolService.ActualizarAsync(model);
                if (resultado)
                {
                    _logger.LogInformation("✅ Rol {Id} actualizado exitosamente", id);
                    TempData["Success"] = "Rol actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "No se pudo actualizar el rol");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar rol {Id}", id);
                ModelState.AddModelError(string.Empty, "Error al actualizar el rol");
            }

            ViewBag.Pantallas = await _rolService.ObtenerPantallasConAsignacionAsync(id);
            return View(model);
        }

        // POST: Roles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("=== INTENTANDO ELIMINAR ROL {Id} ===", id);

                var resultado = await _rolService.EliminarAsync(id);

                if (resultado)
                {
                    _logger.LogInformation("✅ Rol {Id} eliminado exitosamente", id);
                    return Json(new { success = true, message = "Rol eliminado exitosamente" });
                }
                else
                {
                    _logger.LogWarning("❌ No se pudo eliminar el rol {Id}", id);
                    return Json(new { success = false, message = "No se pudo eliminar el rol" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al eliminar rol {Id}", id);
                return Json(new { success = false, message = "Error al eliminar el rol: " + ex.Message });
            }
        }

        // GET: Roles/Buscar
        [HttpGet]
        public async Task<IActionResult> Buscar(string termino)
        {
            try
            {
                var todos = await _rolService.ListarTodosAsync() ?? new List<RolViewModel>();

                if (!string.IsNullOrWhiteSpace(termino))
                {
                    termino = termino.ToLower();
                    todos = todos.Where(r =>
                        (r.Nombre?.ToLower()?.Contains(termino) ?? false)
                    ).ToList();
                }

                return Json(todos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de roles");
                return Json(new List<RolViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            try
            {
                _logger.LogInformation("=== TEST ROLES ===");

                var resultado = await _rolService.ListarTodosAsync();

                if (resultado != null && resultado.Any())
                {
                    return Content($"✅ Roles encontrados: {resultado.Count}");
                }
                else
                {
                    return Content("❌ No se encontraron roles");
                }
            }
            catch (Exception ex)
            {
                return Content($"❌ Error: {ex.Message}");
            }
        }

    }
}