using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace AbstractDataAccess.Models
{
    public class EstadoCore
    {
        [Key]
        public int ID_Estado { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Tipo_Entidad { get; set; }
    }
}
