using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class Bitacoratransaccionresponse
    {
        public DateTime Fecha { get; set; }
        public string TelefonoOrigen { get; set; } = string.Empty;
        public string TelefonoDestino { get; set; } = string.Empty;
        public int  Monto { get; set; }
    }
}
