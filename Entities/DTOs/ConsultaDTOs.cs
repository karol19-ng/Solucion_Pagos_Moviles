using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    // SRV13 - Consulta saldo
    public class ConsultaSaldoRequest
    {
        public string Telefono { get; set; }
        public string Identificacion { get; set; }
    }

    public class ConsultaSaldoResponse
    {
        public decimal Saldo { get; set; }
    }

    // SRV11 - Últimos movimientos
    public class UltimosMovimientosRequest
    {
        public string Telefono { get; set; }
        public string Identificacion { get; set; }
    }

    public class MovimientoDTO
    {
        public long Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal SaldoNuevo { get; set; }
    }

    // SRV15 - Consulta saldo core
    public class CoreConsultaSaldoRequest
    {
        public string Identificacion { get; set; }
        public string Cuenta { get; set; }
    }

    // SRV16 - Consulta movimientos core
    public class CoreConsultaMovimientosRequest
    {
        public string Identificacion { get; set; }
        public string Cuenta { get; set; }
    }

    // SRV14 - Transacción core
    public class CoreTransaccionRequest
    {
        public string Identificacion { get; set; }
        public string Tipo_Movimiento { get; set; } // "DEBITO" o "CREDITO"
        public decimal Monto { get; set; }
    }
}
