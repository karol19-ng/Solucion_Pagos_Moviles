using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractDataAccess.Models
{
    public class Parametro
    {
        [Key]
        public string ID_Parametro { get; set; }

        [Required]
        public string Valor { get; set; }

        public int? ID_Estado { get; set; }
        public DateTime? Fecha_Creacion { get; set; }
    }
}
