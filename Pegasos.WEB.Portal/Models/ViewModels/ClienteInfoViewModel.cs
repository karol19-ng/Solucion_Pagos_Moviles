namespace Pegasos.WEB.Portal.Models.ViewModels
{
    public class ClienteInfoViewModel
    {
        public int IdCliente { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = string.Empty;
        public List<CuentaInfoViewModel> Cuentas { get; set; } = new();
    }

    public class CuentaInfoViewModel
    {
        public string NumeroCuenta { get; set; } = string.Empty;
        public string TipoCuenta { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}