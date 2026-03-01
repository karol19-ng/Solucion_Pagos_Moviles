using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entities.DTOs;

namespace Services.Interfaces
{
    public interface ICoreBancarioService
    {
        Task<CoreOperacionResponse> AplicarTransaccionAsync(CoreTransaccionRequest request, string usuarioEjecutor);
        Task<CoreOperacionResponse> ConsultarSaldoAsync(CoreConsultaSaldoRequest request);
        Task<List<MovimientoDTO>> ConsultarMovimientosAsync(CoreConsultaMovimientosRequest request);
        Task<bool> ClienteExisteAsync(string identificacion);
    }
}
