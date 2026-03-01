using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AbstractDataAccess.Models
{
    public class TablaPantalla
    {
        [Key]
        public int ID_Pantalla { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Descripcion { get; set; }

        [Required]
        public string Ruta { get; set; }

        public int Estado { get; set; }
    }
}
