using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public interface IEntidadService
    {
        // Listar todas las entidades
        Task<List<EntidadViewModel>?> ListarTodosAsync();

        // Obtener por ID
        Task<EntidadViewModel?> ObtenerPorIdAsync(int id);

        // Crear entidad
        Task<(bool Exito, string Mensaje, int? EntidadId)> CrearAsync(CrearEntidadViewModel model);

        // Editar entidad
        Task<bool> ActualizarAsync(EditarEntidadViewModel model);

        // Eliminar entidad
        Task<bool> EliminarAsync(int id);

        // Buscar entidades por identificador
        Task<List<EntidadViewModel>?> BuscarAsync(string termino);
    }
}