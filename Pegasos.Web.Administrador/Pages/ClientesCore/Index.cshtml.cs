using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;

namespace Pegasos.Web.Administrador.Pages.ClientesCore
{
    public class IndexModel : PageModel
    {
        private readonly IClienteCoreService _clienteService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IClienteCoreService clienteService, ILogger<IndexModel> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        public List<ClienteCoreViewModel> Clientes { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        private const int RegistrosPorPagina = 10;

        public async Task OnGetAsync()
        {
            await CargarClientes();
        }

        private async Task CargarClientes()
        {
            try
            {
                var todos = await _clienteService.ListarTodosAsync();
                if (todos != null)
                {
                    TotalRegistros = todos.Count;
                    TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)RegistrosPorPagina);

                    Clientes = todos
                        .Skip((PaginaActual - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar clientes");
                TempData["Error"] = "Error al cargar los clientes";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
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

            return RedirectToPage();
        }
    }
}