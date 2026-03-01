using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.DTOs;

namespace Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<List<UsuarioResponse>> GetAllAsync();
        Task<UsuarioResponse> GetByIdAsync(int id);
        Task<List<UsuarioResponse>> GetByFilterAsync(string identificacion, string nombre, string tipo);
        Task<UsuarioResponse> CreateAsync(UsuarioRequest request, string usuarioEjecutor);
        Task<UsuarioResponse> UpdateAsync(int id, UsuarioRequest request, string usuarioEjecutor);
        Task<bool> DeleteAsync(int id, string usuarioEjecutor);
    }
}
