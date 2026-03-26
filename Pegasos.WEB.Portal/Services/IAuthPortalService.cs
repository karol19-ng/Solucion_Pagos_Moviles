using Pegasos.WEB.Portal.Models.ViewModels;

namespace Pegasos.WEB.Portal.Services
{
    public interface IAuthPortalService
    {
        Task<AuthResult?> LoginAsync(string username, string password);
        Task<bool> ExtendSessionAsync(string accessToken);
    }
}