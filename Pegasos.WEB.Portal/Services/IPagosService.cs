using Pegasos.WEB.Portal.Models.InputModels;
using Pegasos.WEB.Portal.Models.ViewModels;

namespace Pegasos.WEB.Portal.Services
{
    public interface IPagosService
    {
        // PTL5 - Inscripción
        Task<InscripcionResult?> InscribirAsync(InscribirInput input, string token);

        // PTL6 - Desinscripción
        Task<InscripcionResult?> DesinscribirAsync(DesinscribirInput input, string token);

        // PTL9 - Transferencia
        Task<TransferenciaResult?> RealizarTransferenciaAsync(TransferirInput input, string token);
    }
}