using Pegasos.WEB.Portal.Models.InputModels;
using Pegasos.WEB.Portal.Models.ViewModels;

namespace Pegasos.WEB.Portal.Services
{
    public interface IPagosService
    {
        Task<InscripcionResult?> InscribirAsync(InscribirInput input, string token);
        Task<InscripcionResult?> DesinscribirAsync(DesinscribirInput input, string token);
        Task<TransferenciaResult?> RealizarTransferenciaAsync(TransferirInput input, string token);
    }
}