using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class CuentasCoreController : Controller
    {
        private readonly ICuentaCoreService _cuentaService;
        private readonly IClienteCoreService _clienteService;
        private readonly ILogger<CuentasCoreController> _logger;

        public CuentasCoreController(
            ICuentaCoreService cuentaService,
            IClienteCoreService clienteService,
            ILogger<CuentasCoreController> logger)
        {
            _cuentaService = cuentaService;
            _clienteService = clienteService;
            _logger = logger;
        }

        // GET: CuentasCore - Listar todas (con paginación)
        public async Task<IActionResult> Index(int pagina = 1)
        {
            try
            {
                var todos = await _cuentaService.ListarTodosAsync() ?? new List<CuentaCoreViewModel>();

                _logger.LogInformation("Total de cuentas obtenidas: {Count}", todos.Count);

                const int registrosPorPagina = 10;
                var totalRegistros = todos.Count;
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

                var cuentas = todos
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToList();

                ViewBag.PaginaActual = pagina;
                ViewBag.TotalPaginas = totalPaginas;
                ViewBag.TotalRegistros = totalRegistros;

                return View(cuentas);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Sesión expirada. Por favor inicie sesión nuevamente.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index de cuentas core");
                TempData["Error"] = "Error al cargar las cuentas";
                return View(new List<CuentaCoreViewModel>());
            }
        }

        // GET: CuentasCore/Create - Mostrar formulario de creación
        public async Task<IActionResult> Create(string? identificacionCliente = null)
        {
            try
            {
                ViewBag.TiposCuenta = await _cuentaService.ObtenerTiposCuentaAsync()
                    ?? new List<string> { "CORRIENTE", "AHORROS" };

                var model = new CrearCuentaCoreViewModel();
                if (!string.IsNullOrWhiteSpace(identificacionCliente))
                {
                    model.ClienteIdentificacion = identificacionCliente;

                    // Verificar que el cliente existe
                    var cliente = await _clienteService.ObtenerPorIdentificacionAsync(identificacionCliente);
                    if (cliente == null)
                    {
                        TempData["Warning"] = $"El cliente con identificación {identificacionCliente} no existe. Debe crearlo primero.";
                    }
                    else
                    {
                        ViewBag.ClienteInfo = cliente;
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de creación");
                TempData["Error"] = "Error al cargar el formulario";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: CuentasCore/Create - Procesar creación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearCuentaCoreViewModel model)
        {
            // ========== LOGS DE DEPURACIÓN ==========
            _logger.LogInformation("=== DATOS RECIBIDOS EN CREATE ===");

            if (model == null)
            {
                _logger.LogWarning("❌ Modelo es NULL");
                return Json(new { success = false, message = "Datos no válidos" });
            }

            _logger.LogInformation("ClienteIdentificacion: '{Identificacion}'", model.ClienteIdentificacion ?? "null");
            _logger.LogInformation("TipoCuenta: '{TipoCuenta}'", model.TipoCuenta ?? "null");

            // Verificar el Request directamente
            _logger.LogInformation("Request Form Keys: {Keys}", string.Join(", ", Request.Form.Keys));
            foreach (var key in Request.Form.Keys)
            {
                _logger.LogInformation("Form[{Key}] = {Value}", key, Request.Form[key]);
            }
            // ========================================

            try
            {
                _logger.LogInformation("Intentando crear cuenta - Cliente: {Identificacion}, Tipo: {Tipo}",
                    model?.ClienteIdentificacion, model?.TipoCuenta);

                if (model == null)
                {
                    _logger.LogWarning("Modelo es null");
                    return Json(new { success = false, message = "Datos no válidos" });
                }

                if (string.IsNullOrWhiteSpace(model.ClienteIdentificacion))
                {
                    return Json(new { success = false, message = "La identificación del cliente es requerida" });
                }

                if (string.IsNullOrWhiteSpace(model.TipoCuenta))
                {
                    _logger.LogWarning("❌ TipoCuenta está vacío o es null");
                    return Json(new { success = false, message = "El tipo de cuenta es requerido" });
                }

                var (exito, mensaje, cuentaId) = await _cuentaService.CrearAsync(model);
                if (exito)
                {
                    _logger.LogInformation("Cuenta creada exitosamente. ID: {CuentaId}", cuentaId);
                    return Json(new { success = true, message = mensaje });
                }

                _logger.LogWarning("No se pudo crear la cuenta: {Mensaje}", mensaje);
                return Json(new { success = false, message = mensaje });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuenta");
                return Json(new { success = false, message = "Error al crear la cuenta: " + ex.Message });
            }
        }

        // GET: CuentasCore/Edit/5 - Mostrar formulario de edición
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var cuenta = await _cuentaService.ObtenerPorIdAsync(id);
                if (cuenta == null)
                {
                    TempData["Error"] = "Cuenta no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                var model = new EditarCuentaCoreViewModel
                {
                    Id = cuenta.Id,
                    TipoCuenta = cuenta.TipoCuenta,
                    EstadoId = cuenta.EstadoId ?? 1
                };

                ViewBag.TiposCuenta = await _cuentaService.ObtenerTiposCuentaAsync()
                    ?? new List<string>();
                ViewBag.CuentaInfo = cuenta;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cuenta para editar {Id}", id);
                TempData["Error"] = "Error al cargar la cuenta";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: CuentasCore/Edit/5 - Procesar edición
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditarCuentaCoreViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.TipoCuenta))
            {
                ModelState.AddModelError("TipoCuenta", "El tipo de cuenta es requerido");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.TiposCuenta = await _cuentaService.ObtenerTiposCuentaAsync()
                    ?? new List<string>();
                var cuenta = await _cuentaService.ObtenerPorIdAsync(id);
                ViewBag.CuentaInfo = cuenta;
                return View(model);
            }

            try
            {
                var resultado = await _cuentaService.ActualizarAsync(model);
                if (resultado)
                {
                    TempData["Success"] = "Cuenta actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "No se pudo actualizar la cuenta");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cuenta {Id}", id);
                ModelState.AddModelError(string.Empty, "Error al actualizar la cuenta");
            }

            ViewBag.TiposCuenta = await _cuentaService.ObtenerTiposCuentaAsync()
                ?? new List<string>();
            var cuentaInfo = await _cuentaService.ObtenerPorIdAsync(id);
            ViewBag.CuentaInfo = cuentaInfo;
            return View(model);
        }

        // POST: CuentasCore/Delete/5 - Eliminar cuenta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("=== INTENTANDO ELIMINAR CUENTA ===");
                _logger.LogInformation("ID recibido: {Id}", id);

                var resultado = await _cuentaService.EliminarAsync(id);

                if (resultado)
                {
                    _logger.LogInformation("Cuenta {Id} eliminada exitosamente", id);
                    return Json(new { success = true, message = "Cuenta eliminada exitosamente" });
                }
                else
                {
                    _logger.LogWarning("No se pudo eliminar la cuenta {Id}", id);
                    return Json(new { success = false, message = "No se pudo eliminar la cuenta. Verifique que no tenga movimientos asociados." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cuenta {Id}", id);
                return Json(new { success = false, message = "Error al eliminar la cuenta: " + ex.Message });
            }
        }

        // GET: CuentasCore/Buscar - Búsqueda en tiempo real
        [HttpGet]
        public async Task<IActionResult> Buscar(string termino)
        {
            try
            {
                var todas = await _cuentaService.ListarTodosAsync() ?? new List<CuentaCoreViewModel>();

                if (!string.IsNullOrWhiteSpace(termino))
                {
                    termino = termino.ToLower();
                    todas = todas.Where(c =>
                        (c.NumeroCuenta?.ToLower()?.Contains(termino) ?? false) ||
                        (c.ClienteIdentificacion?.Contains(termino) ?? false) ||
                        (c.ClienteNombre?.ToLower()?.Contains(termino) ?? false)
                    ).ToList();
                }

                return Json(todas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de cuentas");
                return Json(new List<CuentaCoreViewModel>());
            }
        }

        // GET: CuentasCore/ValidarCliente - Validar cliente en tiempo real
        [HttpGet]
        public async Task<IActionResult> ValidarCliente(string valor)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(valor))
                {
                    return Json(new { existe = false, mensaje = "Debe ingresar un valor" });
                }

                ClienteCoreViewModel? cliente = null;

                // Buscar por ID si es numérico
                if (int.TryParse(valor, out int id))
                {
                    cliente = await _clienteService.ObtenerPorIdAsync(id);
                }

                // Si no encontró por ID, buscar por identificación
                if (cliente == null)
                {
                    cliente = await _clienteService.ObtenerPorIdentificacionAsync(valor);
                }

                if (cliente != null)
                {
                    return Json(new
                    {
                        existe = true,
                        cliente = new
                        {
                            cliente.Id,
                            cliente.NombreCompleto,
                            cliente.Identificacion
                        },
                        mensaje = $"✓ Cliente encontrado: {cliente.NombreCompleto} (ID: {cliente.Id})"
                    });
                }

                return Json(new { existe = false, mensaje = $"❌ No existe cliente con identificación/ID '{valor}'" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando cliente");
                return Json(new { existe = false, mensaje = "Error al validar cliente" });
            }
        }

        // GET: CuentasCore/Detalles/5 - Ver detalles de una cuenta
        public async Task<IActionResult> Detalles(int id)
        {
            try
            {
                var cuenta = await _cuentaService.ObtenerPorIdAsync(id);
                if (cuenta == null)
                {
                    TempData["Error"] = "Cuenta no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                return View(cuenta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de la cuenta {Id}", id);
                TempData["Error"] = "Error al cargar los detalles";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}