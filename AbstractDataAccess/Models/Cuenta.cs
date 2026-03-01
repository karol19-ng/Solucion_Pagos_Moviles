using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace AbstractDataAccess.Models
{
    public class Cuenta
    {
        public int ID_Cuenta { get; set; }

        [Key]
        public string Numero_Cuenta { get; set; }
        public int Identificacion_Cliente { get; set; }
        public decimal Saldo { get; set; }
        public int? ID_Estado { get; set; }
    }
}
