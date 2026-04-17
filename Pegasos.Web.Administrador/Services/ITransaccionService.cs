using Pegasos.Web.Administrador.DTOs;
using static Pegasos.Web.Administrador.DTOs.TransaccionDTOs;

namespace Pegasos.Web.Administrador.Services
{
    public interface ITransaccionService
    {
        Task<ReporteTransaccionDTO> ConsultarTransaccionesPorFechaAsync(DateTime fecha);
    }
}