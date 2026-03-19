using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public interface IParametroService
    {
        // Listar todos los parámetros
        Task<List<ParametroViewModel>?> ListarTodosAsync();

        // Obtener por ID (string)
        Task<ParametroViewModel?> ObtenerPorIdAsync(string id);

        // Crear parámetro
        Task<(bool Exito, string Mensaje)> CrearAsync(CrearParametroViewModel model);

        // Editar parámetro
        Task<bool> ActualizarAsync(EditarParametroViewModel model);

        // Eliminar parámetro
        Task<bool> EliminarAsync(string id);

        // Buscar parámetros
        Task<List<ParametroViewModel>?> BuscarAsync(string termino);
    }
}