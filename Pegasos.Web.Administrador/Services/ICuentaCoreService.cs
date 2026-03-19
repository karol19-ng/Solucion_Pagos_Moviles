using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public interface ICuentaCoreService
    {
        // Listar todas las cuentas
        Task<List<CuentaCoreViewModel>?> ListarTodosAsync();

        // Listar por llave primaria
        Task<CuentaCoreViewModel?> ObtenerPorIdAsync(int id);

        // Listar por cliente
        Task<List<CuentaCoreViewModel>?> ObtenerPorClienteAsync(string identificacionCliente);
        Task<List<CuentaCoreViewModel>?> ObtenerPorClienteIdAsync(int clienteId);

        // Crear cuenta
        Task<(bool Exito, string Mensaje, int? CuentaId)> CrearAsync(CrearCuentaCoreViewModel model);

        // Editar cuenta
        Task<bool> ActualizarAsync(EditarCuentaCoreViewModel model);

        // Eliminar cuenta
        Task<bool> EliminarAsync(int id);

        // Obtener tipos de cuenta
        Task<List<string>?> ObtenerTiposCuentaAsync();
    }
}