using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AbstractDataAccess.Models
{
    public class MovimientoCuenta
    {
        [Key]
        public long MovimientoId { get; set; }
        public string Numero_Cuenta { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public decimal Monto { get; set; }
        public string TipoMovimiento { get; set; }
        public string Descripcion { get; set; }
        public decimal? SaldoAnterior { get; set; }
        public decimal? SaldoNuevo { get; set; }
        public string ReferenciaExterna { get; set; }
    }
}
