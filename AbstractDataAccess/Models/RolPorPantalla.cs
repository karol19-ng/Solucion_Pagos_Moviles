using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractDataAccess.Models
{
    public class RolPorPantalla
    {
        [Key]
        public int ID_Rol_Por_Pantalla { get; set; }
        public int ID_Pantalla { get; set; }
        public int ID_Rol { get; set; }
        public string Descripcion { get; set; }
    }
}
