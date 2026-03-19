using System.ComponentModel.DataAnnotations;

namespace Pegasos.Web.Administrador.Models
{
    public class EntidadViewModel
    {
        public int Id { get; set; }
        public string Identificador { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int? EstadoId { get; set; }
        public string EstadoDescripcion { get; set; } = "Activo";
    }

    public class CrearEntidadViewModel
    {
        [Required(ErrorMessage = "El identificador es requerido")]
        [Display(Name = "Identificador")]
        public string Identificador { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre de la Entidad")]
        public string Nombre { get; set; } = string.Empty;
    }

    public class EditarEntidadViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El identificador es requerido")]
        [Display(Name = "Identificador")]
        public string Identificador { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre de la Entidad")]
        public string Nombre { get; set; } = string.Empty;

        public int EstadoId { get; set; } = 1;
    }

    public class EntidadResponse
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public EntidadViewModel? Entidad { get; set; }
        public List<EntidadViewModel>? Entidades { get; set; }
    }
}