using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace AbstractDataAccess.Models
{
    public class Afiliacion
    {
        [Key]
        public long Afiliacion_ID { get; set; }
        public string Numero_Cuenta { get; set; }
        public string Identificacion_Usuario { get; set; }
        public string Telefono { get; set; }
        public int ID_Estado { get; set; }
        public DateTime? Fecha_Afiliacion { get; set; }
        public DateTime? Fecha_Actualizacion { get; set; }
    }
}
