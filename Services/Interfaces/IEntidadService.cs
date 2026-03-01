using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.DTOs;

namespace Services.Interfaces
{
    public interface IEntidadService
    {
        Task<List<EntidadResponse>> GetAllAsync();
        Task<EntidadResponse> GetByIdAsync(int id);
        Task<EntidadResponse> CreateAsync(EntidadRequest request, string usuarioEjecutor);
        Task<EntidadResponse> UpdateAsync(int id, EntidadRequest request, string usuarioEjecutor);
        Task<bool> DeleteAsync(int id, string usuarioEjecutor);
    }
}
