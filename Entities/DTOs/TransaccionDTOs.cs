using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    // SRV7 - Recibir transacción
    public class RecibirTransaccionRequest
    {
        public int Entidad_origen { get; set; }
        public int Entidad_destino { get; set; }
        public string Telefono_origen { get; set; }
        public string Nombre_Origen { get; set; }
        public string Telefono_destino { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
    }

    // SRV8 - Enviar transacción
    public class EnviarTransaccionRequest
    {
        public int Entidad_origen { get; set; }
        public string Telefono_origen { get; set; }
        public string Nombre_Origen { get; set; }
        public string Telefono_destino { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
    }

    // SRV12 - Ruteo de transacciones
    public class RouteTransactionRequest
    {
        public string Telefono_origen { get; set; }
        public string Nombre_Origen { get; set; }
        public string Telefono_destino { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
    }

    // Respuesta estándar transacciones (exactamente como pide el documento)
    public class TransaccionResponse
    {
        public int codigo { get; set; }
        public string descripcion { get; set; }
    }
}
