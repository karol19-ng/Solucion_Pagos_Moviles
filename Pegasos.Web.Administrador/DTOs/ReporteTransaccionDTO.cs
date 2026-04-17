

namespace Pegasos.WEB.Administrador.DTOs
{

    public class ReporteTransaccionItem
    {
        public DateTime Fecha { get; set; }
        public string TelefonoOrigen { get; set; }
        public string TelefonoDestino { get; set; }
        public decimal Monto { get; set; }
    }

    public class ReporteTransaccionResponse
    {
        public DateTime FechaConsultada { get; set; }
        public List<ReporteTransaccionItem> Transacciones { get; set; } = new();
        public decimal TotalMonto { get; set; }
        public int TotalTransacciones { get; set; }
    }
}