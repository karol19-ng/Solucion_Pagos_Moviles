namespace BitacoraService.DTOs
{
    public class Bitacoratransaccionresponse
    {
        public DateTime Fecha { get; set; }
        public string TelefonoOrigen { get; set; } = string.Empty;
        public string TelefonoDestino { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}
