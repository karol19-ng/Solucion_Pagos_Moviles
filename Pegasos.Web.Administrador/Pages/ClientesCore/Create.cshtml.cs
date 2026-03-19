using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Services;

namespace Pegasos.Web.Administrador.Pages.ClientesCore
{
    public class CreateModel : PageModel
    {
        private readonly IClienteCoreService _clienteService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IClienteCoreService clienteService, ILogger<CreateModel> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        [BindProperty]
        public CrearClienteCoreViewModel Cliente { get; set; } = new();

        public List<SelectListItem> TiposIdentificacion { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await CargarTiposIdentificacion();
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
                var resultado = await _clienteService.CrearAsync(Cliente);
                if (resultado)
                {
                    TempData["Success"] = "Cliente creado exitosamente";
                    return RedirectToPage("./Index");
                }
                else
                {
                    ErrorMessage = "No se pudo crear el cliente. Verifique que la identificación no esté duplicada.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente");
                ErrorMessage = "Error al crear el cliente";
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