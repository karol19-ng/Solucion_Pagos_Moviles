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

        // GET: CuentasCore/Cliente/{identificacion} - Listar cuentas por cliente
        public async Task<IActionResult> PorCliente(string identificacion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(identificacion))
                {
                    TempData["Error"] = "Debe proporcionar una identificación de cliente";
                    return RedirectToAction(nameof(Index));
                }

                var cliente = await _clienteService.ObtenerPorIdentificacionAsync(identificacion);
                if (cliente == null)
                {
                    TempData["Error"] = $"No se encontró un cliente con identificación {identificacion}";
                    return RedirectToAction(nameof(Index));
                }

                var cuentas = await _cuentaService.ObtenerPorClienteAsync(identificacion) ?? new List<CuentaCoreViewModel>();

                ViewBag.Cliente = cliente;
                return View(cuentas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por cliente {Identificacion}", identificacion);
                TempData["Error"] = "Error al cargar las cuentas del cliente";
                return RedirectToAction(nameof(Index));
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
            try
            {
                _logger.LogInformation("Intentando crear cuenta - Cliente: {Identificacion}", //Tipo: {Tipo}
                    model?.ClienteIdentificacion); //model?.TipoCuenta)

                if (model == null)
                {
                    _logger.LogWarning("Modelo es null");
                    ModelState.AddModelError(string.Empty, "Datos no válidos");
                    ViewBag.TiposCuenta = await _cuentaService.ObtenerTiposCuentaAsync();
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.ClienteIdentificacion))
                {
                    ModelState.AddModelError("ClienteIdentificacion", "La identificación del cliente es requerida");
                }

                //if (string.IsNullOrWhiteSpace(model.TipoCuenta))
               // {
                 //   ModelState.AddModelError("TipoCuenta", "El tipo de cuenta es requerido");
                //}

                //if (!ModelState.IsValid)
                //{
                 //   _logger.LogWarning("ModelState inválido");
                 //   ViewBag.TiposCuenta = await _cuentaService.ObtenerTiposCuentaAsync();
                 //   return View(model);
                //}

                var (exito, mensaje, cuentaId) = await _cuentaService.CrearAsync(model);
                if (exito)
                {
                    _logger.LogInformation("Cuenta creada exitosamente. ID: {CuentaId}", cuentaId);
                    TempData["Success"] = mensaje;
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("No se pudo crear la cuenta: {Mensaje}", mensaje);
                TempData["Error"] = mensaje;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuenta");
                TempData["Error"] = "Error al crear la cuenta: " + ex.Message;
            }

            ViewBag.TiposCuenta = await _cuentaService.ObtenerTiposCuentaAsync();
            return View(model);
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
                    //TipoCuenta = cuenta.TipoCuenta,
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

            //if (string.IsNullOrWhiteSpace(model.TipoCuenta))
            //{
            //    ModelState.AddModelError("TipoCuenta", "El tipo de cuenta es requerido");
            //}

            //if (!ModelState.IsValid)
            //{
            //    ViewBag.TiposCuenta = await _cuentaService.ObtenerTiposCuentaAsync()
            //        ?? new List<string>();
             //   return View(model);
            //}

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
                    return Json(new { success = true, message = "✅ Cuenta eliminada exitosamente" });
                }
                else
                {
                    _logger.LogWarning("No se pudo eliminar la cuenta {Id}", id);
                    return Json(new { success = false, message = "❌ No se pudo eliminar la cuenta. Verifique que no tenga movimientos asociados." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cuenta {Id}", id);
                return Json(new { success = false, message = "❌ Error al eliminar la cuenta: " + ex.Message });
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

        // GET: CuentasCore/BuscarPorCliente - Buscar cuentas por cliente
        [HttpGet]
        public async Task<IActionResult> BuscarPorCliente(string identificacion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(identificacion))
                {
                    return Json(new List<CuentaCoreViewModel>());
                }

                var cuentas = await _cuentaService.ObtenerPorClienteAsync(identificacion)
                    ?? new List<CuentaCoreViewModel>();

                return Json(cuentas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de cuentas por cliente");
                return Json(new List<CuentaCoreViewModel>());
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

        [HttpGet]
        public async Task<IActionResult> TestCreate()
        {
            try
            {
                // Buscar un cliente existente
                var clientes = await _clienteService.ListarTodosAsync();
                var primerCliente = clientes?.FirstOrDefault();

                if (primerCliente == null)
                {
                    return Content("❌ No hay clientes para crear una cuenta de prueba");
                }

                var model = new CrearCuentaCoreViewModel
                {
                    ClienteIdentificacion = primerCliente.Identificacion,
                    //TipoCuenta = "AHORROS"
                };

                _logger.LogInformation("Probando creación directa de cuenta");
                var (exito, mensaje, cuentaId) = await _cuentaService.CrearAsync(model);

                if (exito)
                {
                    return Content($"✅ Cuenta creada exitosamente. ID: {cuentaId}");
                }
                else
                {
                    return Content($"❌ Falló la creación: {mensaje}");
                }
            }
            catch (Exception ex)
            {
                return Content($"❌ Error: {ex.Message}");
            }
        }
    }
}