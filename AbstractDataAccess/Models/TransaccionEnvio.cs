using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace AbstractDataAccess.Models
{
    public class TransaccionEnvio
    {
        [Key]
        public int ID_Transaccion { get; set; }
        public int ID_Entidad_Origen { get; set; }
        public int ID_EntidadDestino { get; set; }
        public string Telefono_Origen { get; set; }
        public string Nombre_Origen { get; set; }
        public string Telefono_Destino { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaEnvio { get; set; }
        public int? Codigo_Respuesta { get; set; }
        public string Mensaje_Respuesta { get; set; }
        public int? ID_Estado { get; set; }
    }
}
