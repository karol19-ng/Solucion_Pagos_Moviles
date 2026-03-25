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

        [ForeignKey("Rol")]
        [Column("ID_Rol")]
        public int ID_Rol { get; set; }

        [ForeignKey("Pantalla")]
        [Column("ID_Pantalla")]
        public int ID_Pantalla { get; set; }

        //public string? Descripcion { get; set; } 

        // Propiedades de navegación
        public virtual Rol? Rol { get; set; }
        public virtual TablaPantalla? Pantalla { get; set; }
    }
}
