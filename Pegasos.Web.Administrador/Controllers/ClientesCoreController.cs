using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;

namespace Pegasos.Web.Administrador.Controllers
{
    [Authorize]
    public class ClientesCoreController : Controller
    {
        private readonly IClienteCoreService _clienteService;
        private readonly ILogger<ClientesCoreController> _logger;

        public ClientesCoreController(
            IClienteCoreService clienteService,
            ILogger<ClientesCoreController> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        // GET: ClientesCore
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index de clientes core");
                TempData["Error"] = "Error al cargar los clientes";
                return View(new List<ClienteCoreViewModel>());
            }
        }

        // GET: ClientesCore/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.TiposIdentificacion = await _clienteService.ObtenerTiposIdentificacionAsync()
                ?? new List<string>();
            return View();
        }

        // POST: ClientesCore/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearClienteCoreViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TiposIdentificacion = await _clienteService.ObtenerTiposIdentificacionAsync()
                    ?? new List<string>();
                return View(model);
            }

            try
            {
                var resultado = await _clienteService.CrearAsync(model);
                if (resultado)
                {
                    TempData["Success"] = "Cliente creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "No se pudo crear el cliente. Verifique que la identificación no esté duplicada.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente");
                ModelState.AddModelError(string.Empty, "Error al crear el cliente");
            }

            ViewBag.TiposIdentificacion = await _clienteService.ObtenerTiposIdentificacionAsync()
                ?? new List<string>();
            return View(model);
        }

        // GET: ClientesCore/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _clienteService.ObtenerPorIdAsync(id);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado";
                return RedirectToAction(nameof(Index));
            }

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

        // POST: ClientesCore/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditarClienteCoreViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.TiposIdentificacion = await _clienteService.ObtenerTiposIdentificacionAsync()
                    ?? new List<string>();
                return View(model);
            }

            try
            {
                var resultado = await _clienteService.ActualizarAsync(model);
                if (resultado)
                {
                    TempData["Success"] = "Cliente actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "No se pudo actualizar el cliente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente {Id}", id);
                ModelState.AddModelError(string.Empty, "Error al actualizar el cliente");
            }

            ViewBag.TiposIdentificacion = await _clienteService.ObtenerTiposIdentificacionAsync()
                ?? new List<string>();
            return View(model);
        }

        // POST: ClientesCore/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var resultado = await _clienteService.EliminarAsync(id);
                if (resultado)
                {
                    TempData["Success"] = "Cliente eliminado exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar el cliente. Verifique que no tenga cuentas asociadas.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente {Id}", id);
                TempData["Error"] = "Error al eliminar el cliente";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ClientesCore/Buscar
        public async Task<IActionResult> Buscar(string termino)
        {
            try
            {
                var todos = await _clienteService.ListarTodosAsync() ?? new List<ClienteCoreViewModel>();

                if (!string.IsNullOrWhiteSpace(termino))
                {
                    termino = termino.ToLower();
                    todos = todos.Where(c =>
                        c.NombreCompleto.ToLower().Contains(termino) ||
                        c.Identificacion.Contains(termino)
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
    }
}