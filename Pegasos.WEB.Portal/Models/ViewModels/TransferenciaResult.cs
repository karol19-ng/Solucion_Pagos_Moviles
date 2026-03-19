namespace Pegasos.WEB.Portal.Models.ViewModels
{
    public class TransferenciaResult
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string? Comprobante { get; set; }
        public decimal? Saldo { get; set; }
    }
}