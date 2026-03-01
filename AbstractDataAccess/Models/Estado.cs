using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractDataAccess.Models
{
    public class Estado
    {
        [Key]
        public int ID_Estado { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }
}
