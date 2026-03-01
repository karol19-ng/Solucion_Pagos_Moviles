using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; 
namespace AbstractDataAccess.Models
{
    public class Entidad
    {
        [Key]
        public int ID_Entidad { get; set; }

        [Required]
        public string Nombre_Institucion { get; set; }

        public int? ID_Estado { get; set; }
        public DateTime? Fecha_Creacion { get; set; }
    }
}
