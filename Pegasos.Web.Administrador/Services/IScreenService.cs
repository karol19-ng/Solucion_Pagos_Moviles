using Pegasos.Web.Administrador.Models;

namespace Pegasos.Web.Administrador.Services
{
    public interface IScreenService
    {
        Task<List<PantallaResponse>> GetAllAsync();
        Task<PantallaResponse> GetByIdAsync(int id);
        Task<PantallaResponse> CreateAsync(PantallaRequest request);
        Task<PantallaResponse> UpdateAsync(int id, PantallaRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
