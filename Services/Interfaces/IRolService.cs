using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.DTOs;

namespace Services.Interfaces
{
    public interface IRolService
    {
        Task<List<RolResponse>> GetAllAsync();
        Task<RolResponse> GetByIdAsync(int id);
        Task<RolResponse> CreateAsync(RolRequest request, string usuarioEjecutor);
        Task<RolResponse> UpdateAsync(int id, RolRequest request, string usuarioEjecutor);
        Task<bool> DeleteAsync(int id, string usuarioEjecutor);
    }
}
