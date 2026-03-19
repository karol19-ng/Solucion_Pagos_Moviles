namespace Pegasos.WEB.Portal.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public string HoraIngreso { get; set; } = string.Empty;
        public List<CuentaAsociadaViewModel> CuentasAsociadas { get; set; } = new();
        public decimal SaldoTotal => CuentasAsociadas.Sum(c => c.Saldo);
        public int TotalCuentas => CuentasAsociadas.Count;
        public int TotalTelefonosAsociados => CuentasAsociadas.Count(c => !string.IsNullOrEmpty(c.Telefono));
    }

    public class CuentaAsociadaViewModel
    {
        public string NumeroCuenta { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public bool Activa { get; set; }
        public string TipoCuenta { get; set; } = string.Empty;
    }
}