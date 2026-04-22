namespace Pegasos.Web.Administrador.DTOs
{
    public class TransaccionDTOs
    {
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

        public class EnviarTransaccionRequest
        {
            public int Entidad_origen { get; set; }
            public string Telefono_origen { get; set; }
            public string Nombre_Origen { get; set; }
            public string Telefono_destino { get; set; }
            public decimal Monto { get; set; }
            public string Descripcion { get; set; }
        }

        public class RouteTransactionRequest
        {
            public string Telefono_origen { get; set; }
            public string Nombre_Origen { get; set; }
            public string Telefono_destino { get; set; }
            public decimal Monto { get; set; }
            public string Descripcion { get; set; }
        }

        public class TransaccionResponse
        {
            public int codigo { get; set; }
            public string descripcion { get; set; }
        }
        // SA12 - Reporte diario
        public class ReporteTransaccionItem
        {
            public DateTime Fecha { get; set; }
            public string TelefonoOrigen { get; set; }
            public string TelefonoDestino { get; set; }
            public decimal Monto { get; set; }
        }

        public class ReporteTransaccionDTO
        {
            public DateTime FechaConsultada { get; set; }
            public List<ReporteTransaccionItem> Transacciones { get; set; }
            public decimal TotalMonto { get; set; }
            public int TotalTransacciones { get; set; }
        }
    }
}
