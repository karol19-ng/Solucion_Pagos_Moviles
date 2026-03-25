using System.ComponentModel.DataAnnotations;

namespace Pegasos.Web.Administrador.Models
{
    public class ClienteCoreViewModel
    {
        public int Id { get; set; }
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Display(Name = "Fecha de nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        public int? EstadoId { get; set; }
        public string EstadoDescripcion { get; set; } = "Activo";
    }

    public class CrearClienteCoreViewModel
    {
        [Required(ErrorMessage = "El tipo de identificación es requerido")]
        public string TipoIdentificacion { get; set; } = "FISICA";

        [Required(ErrorMessage = "La identificación es requerida")]
        [Display(Name = "Número de Identificación")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es requerido")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [Display(Name = "Teléfono")]
        [RegularExpression(@"^[0-9\-\(\)\s]+$", ErrorMessage = "El teléfono solo puede contener números, guiones y paréntesis")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaNacimiento { get; set; } = DateTime.Now.AddYears(-18);
    }

    public class EditarClienteCoreViewModel
    {
        public int Id { get; set; }
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;

        [Display(Name = "Teléfono")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        public string Telefono { get; set; } = string.Empty;

        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaNacimiento { get; set; }

        public int EstadoId { get; set; } = 1;
    }

    public class ClienteCoreResponse
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public ClienteCoreViewModel? Cliente { get; set; }
        public List<ClienteCoreViewModel>? Clientes { get; set; }
    }
}