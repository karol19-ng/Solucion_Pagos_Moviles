using Pegasos.WEB.Portal.Models.ViewModels;

namespace Pegasos.WEB.Portal.Services
{
    public interface ICoreClienteService
    {
        Task<ClienteInfoViewModel?> ObtenerInfoClienteAsync(string identificacion, string token);
    }
}