using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AbstractDataAccess.Models
{
    public class Usuario
    {
        [Key]
        public int ID_Usuario { get; set; }

        [Required]
        public string Nombre_Completo { get; set; }

        [Required]
        public string Tipo_Identificacion { get; set; }

        [Required]
        public string Identificacion { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Telefono { get; set; }

        [Required]
        [Column("Usuario")]
        public string NombreUsuario { get; set; }

        [Required]
        public string Contraseña { get; set; }

        public int ID_Estado { get; set; }
        public int ID_Rol { get; set; }
        public DateTime Fecha_Creacion { get; set; }

        [ForeignKey("ID_Rol")]
        public virtual Rol Rol { get; set; }
    }
}
