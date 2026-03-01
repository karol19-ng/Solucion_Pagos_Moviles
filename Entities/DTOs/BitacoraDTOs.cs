using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class BitacoraRequest
    {
        public string Usuario { get; set; }
        public string Descripcion { get; set; }
        public string Accion { get; set; }
    }

    public class BitacoraResponse
    {
        public long BitacoraId { get; set; }
        public string Usuario { get; set; }
        public string Accion { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Servicio { get; set; }
        public string Resultado { get; set; }
    }

    public class BitacoraRegistroRequest
    {
        public string Usuario { get; set; }
        public string Accion { get; set; }
        public string Descripcion { get; set; }
        public string Servicio { get; set; }
        public string Resultado { get; set; }
    }
}
