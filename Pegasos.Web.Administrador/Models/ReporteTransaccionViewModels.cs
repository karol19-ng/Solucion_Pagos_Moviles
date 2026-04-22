
namespace Pegasos.WEB.Admin.Models
{
    public class ReporteTransaccionItemViewModel
    {
        public DateTime Fecha { get; set; }
        public string TelefonoOrigen { get; set; }
        public string TelefonoDestino { get; set; }
        public decimal Monto { get; set; }
    }

    public class ReporteTransaccionViewModel
    {
        public DateTime FechaConsultada { get; set; }
        public List<ReporteTransaccionItemViewModel> Transacciones { get; set; } = new();
        public decimal TotalMonto { get; set; }
        public int TotalTransacciones { get; set; }
    }
}