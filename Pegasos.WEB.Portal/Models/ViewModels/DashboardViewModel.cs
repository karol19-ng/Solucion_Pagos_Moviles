namespace Pegasos.WEB.Portal.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = "Cliente";
        public DateTime FechaIngreso { get; set; }
        public string HoraIngreso { get; set; } = string.Empty;
        public List<CuentaAsociadaViewModel> CuentasAsociadas { get; set; } = new();
    }

    public class CuentaAsociadaViewModel
    {
        public string NumeroCuenta { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public bool Activa { get; set; }
    }
}