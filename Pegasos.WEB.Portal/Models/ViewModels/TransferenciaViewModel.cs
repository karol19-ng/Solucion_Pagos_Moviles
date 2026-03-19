namespace Pegasos.WEB.Portal.Models.ViewModels
{
    public class TransferenciaViewModel
    {
        public string TelefonoOrigen { get; set; } = string.Empty;
        public string NombreOrigen { get; set; } = string.Empty;
        public string TelefonoDestino { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string? Comprobante { get; set; }
    }
}