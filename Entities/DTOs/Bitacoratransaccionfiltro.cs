using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class Bitacoratransaccionfiltro
    {
        /// <summary>
        /// Filtros opcionales para consultar transacciones en la bitácora.
        /// </summary>

        public DateTime? Fecha { get; set; }
        public string? TelefonoOrigen { get; set; }
        public string? TelefonoDestino { get; set; }
        public decimal? Monto { get; set; }

    }
}
