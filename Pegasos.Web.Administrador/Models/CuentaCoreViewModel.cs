using System.ComponentModel.DataAnnotations;

namespace Pegasos.Web.Administrador.Models
{
    public class CuentaCoreViewModel
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteIdentificacion { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string NumeroCuenta { get; set; } = string.Empty;
        public string TipoCuenta { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public int? EstadoId { get; set; }
        public string EstadoDescripcion { get; set; } = "Activo";
        public DateTime FechaApertura { get; set; }
    }

    public class CrearCuentaCoreViewModel
    {
        [Required(ErrorMessage = "La identificación del cliente es requerida")]
        [Display(Name = "Identificación del Cliente")]
        public string ClienteIdentificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de cuenta es requerido")]
        [Display(Name = "Tipo de Cuenta")]
        public string TipoCuenta { get; set; } = string.Empty;
    }

    public class EditarCuentaCoreViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El tipo de cuenta es requerido")]
        [Display(Name = "Tipo de Cuenta")]
        public string TipoCuenta { get; set; } = string.Empty;

        public int EstadoId { get; set; } = 1;
    }

    public class CuentaCoreResponse
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public CuentaCoreViewModel? Cuenta { get; set; }
        public List<CuentaCoreViewModel>? Cuentas { get; set; }
    }

    public class CuentasPorClienteResponse
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public List<CuentaCoreViewModel>? Cuentas { get; set; }
    }
}