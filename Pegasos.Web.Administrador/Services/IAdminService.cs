
using Pegasos.WEB.Admin.Models;

namespace Pegasos.WEB.Admin.Services
{
    public interface IAdminService
    {
       
        Task<ReporteTransaccionViewModel?> ObtenerReporteTransaccionesAsync(DateTime fecha, string token);
    }
}