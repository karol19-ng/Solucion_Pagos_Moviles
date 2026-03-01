using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractDataAccess.Models
{
    public class InicioSesion
    {
        [Key]
        public int ID_Session { get; set; }
        public int ID_Usuario { get; set; }
        public string JWT_Token { get; set; }
        public string Refresh_Token { get; set; }
        public DateTime? Fecha_Inico { get; set; }
        public DateTime? Fecha_Expiracion_Token { get; set; }
        public DateTime? Fecha_Expiracion_Refresh { get; set; }
        public int? ID_Estado { get; set; }

        [ForeignKey("ID_Usuario")]
        public virtual Usuario UsuarioNavigation { get; set; }
    }
}
