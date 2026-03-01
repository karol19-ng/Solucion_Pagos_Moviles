using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.DTOs;

namespace Services.Interfaces
{
    public interface IAfiliacionService
    {
        Task<AfiliacionResponse> InscribirAsync(AfiliacionRequest request, string usuarioEjecutor);
        Task<AfiliacionResponse> DesinscribirAsync(AfiliacionRequest request, string usuarioEjecutor);
        Task<ConsultaSaldoResponse> ConsultarSaldoAsync(ConsultaSaldoRequest request);
        Task<List<MovimientoDTO>> ConsultarMovimientosAsync(UltimosMovimientosRequest request);
    }
}
