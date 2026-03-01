using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace AbstractDataAccess.Models
{
    public class TipoIdentificacion
    {
        [Key]
        public int ID_Identificacion { get; set; }
        public string Tipo_Identificacion { get; set; }
        public string Detalle_Identificacion { get; set; }
    }
}
