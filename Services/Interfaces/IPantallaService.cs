using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.DTOs;

namespace Services.Interfaces
{
    public interface IPantallaService
    {
        Task<List<PantallaResponse>> GetAllAsync();
        Task<PantallaResponse> GetByIdAsync(int id);
        Task<PantallaResponse> CreateAsync(PantallaRequest request, string usuarioEjecutor);
        Task<PantallaResponse> UpdateAsync(int id, PantallaRequest request, string usuarioEjecutor);
        Task<bool> DeleteAsync(int id, string usuarioEjecutor);
    }
}