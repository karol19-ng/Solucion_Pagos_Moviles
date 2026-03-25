using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace AbstractDataAccess.Models
{
    public class Rol
    {
        [Key]
        public int ID_Rol { get; set; }

        [Required]
        public string Nombre { get; set; } = null!;

        public string Descripcion { get; set; } = null!;

        public virtual ICollection<RolPorPantalla> RolPorPantallas { get; set; } = new List<RolPorPantalla>();
    }
}
