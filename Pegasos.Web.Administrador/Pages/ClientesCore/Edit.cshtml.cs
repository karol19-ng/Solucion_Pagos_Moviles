using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;

namespace Pegasos.Web.Administrador.Pages.ClientesCore
{
    public class EditModel : PageModel
    {
        private readonly IClienteCoreService _clienteService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IClienteCoreService clienteService, ILogger<EditModel> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        [BindProperty]
        public EditarClienteCoreViewModel Cliente { get; set; } = new();

        public List<SelectListItem> TiposIdentificacion { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var cliente = await _clienteService.ObtenerPorIdAsync(id);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado";
                return RedirectToPage("./Index");
            }

            Cliente = new EditarClienteCoreViewModel
            {
                Id = cliente.Id,
                TipoIdentificacion = cliente.TipoIdentificacion,
                Identificacion = cliente.Identificacion,
                NombreCompleto = cliente.NombreCompleto,
                EstadoId = cliente.EstadoId ?? 1
            };

            await CargarTiposIdentificacion();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarTiposIdentificacion();
                return Page();
            }

            try
            {
                var resultado = await _clienteService.ActualizarAsync(Cliente);
                if (resultado)
                {
                    TempData["Success"] = "Cliente actualizado exitosamente";
                    return RedirectToPage("./Index");
                }
                else
                {
                    ErrorMessage = "No se pudo actualizar el cliente. Verifique los datos.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente {Id}", Cliente.Id);
                ErrorMessage = "Error al actualizar el cliente";
            }

            await CargarTiposIdentificacion();
            return Page();
        }

        private async Task CargarTiposIdentificacion()
        {
            var tipos = await _clienteService.ObtenerTiposIdentificacionAsync();
            TiposIdentificacion = tipos?.Select(t => new SelectListItem
            {
                Value = t,
                Text = t
            }).ToList() ?? new List<SelectListItem>();
        }
    }
}