using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.DTOs;

namespace Services.Interfaces
{
    public interface ITransaccionService
    {
        Task<TransaccionResponse> RecibirTransaccionAsync(RecibirTransaccionRequest request, string usuarioEjecutor);
        Task<TransaccionResponse> EnviarTransaccionAsync(EnviarTransaccionRequest request, string usuarioEjecutor);
        Task<TransaccionResponse> RouteTransactionAsync(RouteTransactionRequest request, string usuarioEjecutor);
    }
}
