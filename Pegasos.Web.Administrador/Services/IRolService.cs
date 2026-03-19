using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public interface IRolService
    {
        Task<List<RolViewModel>?> ListarTodosAsync();
        Task<RolViewModel?> ObtenerPorIdAsync(int id);
        Task<List<PantallaAsignadaViewModel>?> ObtenerPantallasConAsignacionAsync(int rolId = 0);
        Task<bool> CrearAsync(CrearRolViewModel model);
        Task<bool> ActualizarAsync(EditarRolViewModel model);
        Task<bool> EliminarAsync(int id);
    }
}