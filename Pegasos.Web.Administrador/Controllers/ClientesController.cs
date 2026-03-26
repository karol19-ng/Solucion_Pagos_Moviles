using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly IClienteCoreService _clienteService;
        private readonly ILogger<ClientesCoreController> _logger;

        public ClientesController(
            IClienteCoreService clienteService,
            ILogger<ClientesCoreController> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        // GET: ClientesCore - Listar todos (con paginación)
        public async Task<IActionResult> Index(int pagina = 1)
        {
            try
            {
                var todos = await _clienteService.ListarTodosAsync() ?? new List<ClienteCoreViewModel>();

                const int registrosPorPagina = 10;
                var totalRegistros = todos.Count;
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

                var clientes = todos
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToList();

                ViewBag.PaginaActual = pagina;
                ViewBag.TotalPaginas = totalPaginas;
                ViewBag.TotalRegistros = totalRegistros;

                return View(clientes);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Sesión expirada. Por favor inicie sesión nuevamente.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index de clientes core");
                TempData["Error"] = "Error al cargar los clientes";
                return View(new List<ClienteCoreViewModel>());
            }
        }

        // GET: ClientesCore/Create - Mostrar formulario de creación
        public async Task<IActionResult> Create()
        {
            try
            {
                // Cargar los tipos de identificación para el dropdown
                ViewBag.TiposIdentificacion = await _clienteService.ObtenerTiposIdentificacionAsync()
                    ?? new List<string> { "FISICA", "JURIDICA", "DIMEX", "NITE" };

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de creación");
                TempData["Error"] = "Error al cargar el formulario";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ClientesCore/Create - Procesar creación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearClienteCoreViewModel model)
        {
            try
            {
                // Log para ver qué datos llegan
                _logger.LogInformation("Intentando crear cliente - Identificacion: {Identificacion}, Nombre: {Nombre}",
                    model?.Identificacion, model?.NombreCompleto);

                // Validaciones manuales
                if (model == null)
                {
                    _logger.LogWarning("Modelo es null");
                    ModelState.AddModelError(string.Empty, "Datos no válidos");
                    ViewBag.TiposIdentificacion = await _clienteService.ObtenerTiposIdentificacionAsync();
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.Identificacion))
                {
                    ModelState.AddModelError("Identificacion", "La identificación es requerida");
                }

                if (string.IsNullOrWhiteSpace(model.NombreCompleto))
                {
                    ModelState.AddModelError("NombreCompleto", "El nombre completo es requerido");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido");
                    ViewBag.TiposIdentificacion = await _clienteService.ObtenerTiposIdentificacionAsync();
                    return View(model);
                }

                var resultado = await _clienteService.CrearAsync(model);
                if (resultado)
                {
                    _logger.LogInformation("Cliente creado exitosamente");
                    TempData["Success"] = "Cliente creado exitosamente"; // ✅ Mensaje de éxito
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("No se pudo crear el cliente");
                TempData["Error"] = "No se pudo crear el cliente. Verifique que la identificación no esté duplicada."; // ❌ Mensaje de error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente");
                TempData["Error"] = "Error al crear el cliente: " + ex.Message; // ❌ Mensaje de error
            }

            ViewBag.TiposIdentificacion = await _clienteService.ObtenerTiposIdentificacionAsync();
            return View(model);
        }

        // GET: ClientesCore/Edit/5 - Mostrar formulario de edición
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation("=== EDITANDO CLIENTE ===");
                _logger.LogInformation("ID recibido: {Id}", id);

                var cliente = await _clienteService.ObtenerPorIdAsync(id);

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente con ID {Id} no encontrado en el servicio", id);
                    TempData["Error"] = "Cliente no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Cliente encontrado: {Nombre}", cliente.NombreCompleto);

                var model = new EditarClienteCoreViewModel
                {
                    Id = cliente.Id,
                    TipoIdentificacion = cliente.TipoIdentificacion,
                    Identificacion = cliente.Identificacion,
                    NombreCompleto = cliente.NombreCompleto,
                    EstadoId = cliente.EstadoId ?? 1
                };

                ViewBag.TiposIdentificacion = await _clienteService.ObtenerTiposIdentificacionAsync()
                    ?? new List<string>();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cliente para editar {Id}", id);
                TempData["Error"] = "Error al cargar el cliente";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ClientesCore/Edit/5 - Procesar edición
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditarClienteCoreViewModel model)
        {
            _logger.LogInformation("=== PROCESANDO EDICIÓN DE CLIENTE ===");
            _logger.LogInformation("ID recibido: {Id}", id);
            _logger.LogInformation("Model - Id: {ModelId}, Nombre: {Nombre}, Identificacion: {Identificacion}, TipoId: {TipoId}, EstadoId: {EstadoId}",
                model?.Id, model?.NombreCompleto, model?.Identificacion, model?.TipoIdentificacion, model?.EstadoId);

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

            // Validaciones manuales
            if (string.IsNullOrWhiteSpace(model.Identificacion))
            {
                _logger.LogWarning("Identificación vacía");
                ModelState.AddModelError("Identificacion", "La identificación es requerida");
            }

            if (string.IsNullOrWhiteSpace(model.NombreCompleto))
            {
                _logger.LogWarning("Nombre completo vacío");
                ModelState.AddModelError("NombreCompleto", "El nombre completo es requerido");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido. Errores: {ErrorCount}", ModelState.ErrorCount);
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning("Error en {Key}: {ErrorMessage}", state.Key, error.ErrorMessage);
                    }
                }

                ViewBag.TiposIdentificacion = await _clienteService.ObtenerTiposIdentificacionAsync()
                    ?? new List<string>();
                return View(model);
            }

            try
            {
                var resultado = await _clienteService.ActualizarAsync(model);
                if (resultado)
                {
                    _logger.LogInformation("✅ Cliente {Id} actualizado exitosamente", id);
                    TempData["Success"] = "Cliente actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("❌ No se pudo actualizar el cliente {Id}", id);
                ModelState.AddModelError(string.Empty, "No se pudo actualizar el cliente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar cliente {Id}", id);
                ModelState.AddModelError(string.Empty, "Error al actualizar el cliente");
            }

            ViewBag.TiposIdentificacion = await _clienteService.ObtenerTiposIdentificacionAsync()
                ?? new List<string>();
            return View(model);
        }

        // POST: ClientesCore/Delete/5 - Eliminar cliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("=== INTENTANDO ELIMINAR CLIENTE ===");
                _logger.LogInformation("ID recibido: {Id}", id);

                var resultado = await _clienteService.EliminarAsync(id);

                if (resultado)
                {
                    _logger.LogInformation("Cliente {Id} eliminado exitosamente", id);
                    return Json(new { success = true, message = "✅ Cliente eliminado exitosamente" });
                }
                else
                {
                    _logger.LogWarning("No se pudo eliminar el cliente {Id}", id);
                    return Json(new { success = false, message = "❌ No se puede eliminar el cliente porque tiene cuentas asociadas. Primero debe eliminar las cuentas del cliente." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente {Id}", id);
                return Json(new { success = false, message = "❌ Error al eliminar el cliente: " + ex.Message });
            }
        }

        // GET: ClientesCore/Buscar - Búsqueda en tiempo real
        [HttpGet]
        public async Task<IActionResult> Buscar(string termino)
        {
            try
            {
                var todos = await _clienteService.ListarTodosAsync() ?? new List<ClienteCoreViewModel>();

                if (!string.IsNullOrWhiteSpace(termino))
                {
                    termino = termino.ToLower();
                    todos = todos.Where(c =>
                        (c.NombreCompleto?.ToLower()?.Contains(termino) ?? false) ||
                        (c.Identificacion?.Contains(termino) ?? false)
                    ).ToList();
                }

                return Json(todos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de clientes");
                return Json(new List<ClienteCoreViewModel>());
            }
        }

        // GET: ClientesCore/Detalles/5 - Ver detalles de un cliente (opcional, si quieres añadir)
        public async Task<IActionResult> Detalles(int id)
        {
            try
            {
                var cliente = await _clienteService.ObtenerPorIdAsync(id);
                if (cliente == null)
                {
                    TempData["Error"] = "Cliente no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del cliente {Id}", id);
                TempData["Error"] = "Error al cargar los detalles";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestCreate()
        {
            try
            {
                var model = new CrearClienteCoreViewModel
                {
                    TipoIdentificacion = "FISICA",
                    Identificacion = "TEST" + DateTime.Now.Ticks,
                    NombreCompleto = "Cliente de Prueba"
                };

                _logger.LogInformation("Probando creación directa");
                var resultado = await _clienteService.CrearAsync(model);

                if (resultado)
                {
                    return Content("✅ Cliente creado exitosamente");
                }
                else
                {
                    return Content("❌ Falló la creación");
                }
            }
            catch (Exception ex)
            {
                return Content($"❌ Error: {ex.Message}");
            }
        }

    }
}