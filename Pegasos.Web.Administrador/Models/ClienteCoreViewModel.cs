using System.ComponentModel.DataAnnotations;

namespace Pegasos.Web.Administrador.Models
{
    public class ClienteCoreViewModel
    {
        public int Id { get; set; }
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
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
    }

    public class EditarClienteCoreViewModel
    {
        public int Id { get; set; }
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
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