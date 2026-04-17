using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AbstractDataAccess.Models
{
    public class Bitacora
    {
        [Key]
        public long BitacoraId { get; set; }
        public string Usuario { get; set; }
        public string Accion { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Servicio { get; set; }
        public string Resultado { get; set; }

        public int Monto { get; set; }
        public int TelefonoDestino { get; set; }
        public int TelefonoOrigen { get; set; }
    }
}
