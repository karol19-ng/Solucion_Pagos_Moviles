using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public interface IClienteCoreService
    {
        Task<List<ClienteCoreViewModel>?> ListarTodosAsync();
        Task<ClienteCoreViewModel?> ObtenerPorIdAsync(int id);
        Task<ClienteCoreViewModel?> ObtenerPorIdentificacionAsync(string identificacion);
        Task<bool> CrearAsync(CrearClienteCoreViewModel model);
        Task<bool> ActualizarAsync(EditarClienteCoreViewModel model);
        Task<bool> EliminarAsync(int id);
        Task<List<string>?> ObtenerTiposIdentificacionAsync();
    }
}