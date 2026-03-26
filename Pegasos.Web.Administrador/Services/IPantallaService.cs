using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public interface IPantallaService
    {
        Task<List<PantallaViewModel>?> ListarTodosAsync();
        Task<PantallaViewModel?> ObtenerPorIdAsync(int id);
        Task<bool> CrearAsync(CrearPantallaViewModel model);
        Task<bool> ActualizarAsync(EditarPantallaViewModel model);
        Task<bool> EliminarAsync(int id);
    }
}