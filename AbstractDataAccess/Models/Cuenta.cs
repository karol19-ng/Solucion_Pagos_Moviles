using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractDataAccess.Models
{
    [Table("Cuentas")]  
    public class Cuenta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID_Cuenta")]
        public int ID_Cuenta { get; set; }

        [Required]
        [StringLength(22)]
        [Column("Numero_Cuenta")]
        public string Numero_Cuenta { get; set; } = "";

        [StringLength(50)]
        [Column("Tipo_Cuenta")]
        public string? Tipo_Cuenta { get; set; } = "Ahorros";

        [Column("Identificacion_Cliente")]
        public int Identificacion_Cliente { get; set; }

        [Column("Saldo")]
        public decimal Saldo { get; set; } = 0;

        [Column("ID_Estado")]
        public int? ID_Estado { get; set; } = 1;

        [ForeignKey("Identificacion_Cliente")]
        public virtual ClienteBanco? Cliente { get; set; }

        [ForeignKey("ID_Estado")]
        public virtual EstadoCore? Estado { get; set; }
    }
}