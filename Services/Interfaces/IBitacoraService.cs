using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.DTOs;

namespace Services.Interfaces
{
    public interface IBitacoraService
    {
        Task RegistrarBitacoraAsync(BitacoraRegistroRequest request);
        Task<List<BitacoraResponse>> ConsultarBitacorasAsync();
        Task<List<BitacoraResponse>> ConsultarPorUsuarioAsync(string usuario);
        Task<List<BitacoraResponse>> ConsultarPorFechaAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}
