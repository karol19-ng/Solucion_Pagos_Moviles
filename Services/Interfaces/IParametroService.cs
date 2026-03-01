using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.DTOs;

namespace Services.Interfaces
{
    public interface IParametroService
    {
        Task<List<ParametroResponse>> GetAllAsync();
        Task<ParametroResponse> GetByIdAsync(string id);
        Task<ParametroResponse> CreateAsync(ParametroRequest request, string usuarioEjecutor);
        Task<ParametroResponse> UpdateAsync(string id, ParametroRequest request, string usuarioEjecutor);
        Task<bool> DeleteAsync(string id, string usuarioEjecutor);
    }
}
