using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractDataAccess.Models
{
    public class ClienteBanco
    {
        [Key]
        public int ID_Cliente { get; set; }
        public string Tipo_Identificacion { get; set; }
        public string Identificacion { get; set; }
        public string Nombre_Completo { get; set; }
        public int? ID_Estado { get; set; }
    }
}
