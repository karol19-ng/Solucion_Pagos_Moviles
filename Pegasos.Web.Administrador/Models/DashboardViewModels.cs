using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
namespace Pegasos.Web.Administrador.Models
{
    public class DashboardViewModels
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public string HoraIngreso { get; set; } = string.Empty;
    }
}
