 using global::Pegasos.Web.Administrador.Models;
using Pegasos.Web.Administrador.Models;

    namespace Pegasos.Web.Administrador.Services
    {
        public interface IBitacoraAdminService
        {
            Task<List<BitacoraItemViewModel>> ConsultarTransaccionesAsync(DateTime? fecha);
        }
    }    
    
