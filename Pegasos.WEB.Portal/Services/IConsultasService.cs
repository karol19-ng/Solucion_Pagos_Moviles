using Pegasos.WEB.Portal.Models.ViewModels;

namespace Pegasos.WEB.Portal.Services
{
    public interface IConsultasService
    {
        Task<SaldoViewModel?> ConsultarSaldoAsync(string telefono, string identificacion, string token);
        Task<MovimientosViewModel?> ConsultarMovimientosAsync(string telefono, string identificacion, string token);
    }
}