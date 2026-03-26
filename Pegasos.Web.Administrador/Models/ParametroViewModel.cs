using System.ComponentModel.DataAnnotations;

namespace Pegasos.Web.Administrador.Models
{
    public class ParametroViewModel
    {
        public string Id { get; set; } = string.Empty; // ID_Parametro es string
        public string Valor { get; set; } = string.Empty;
        public int? EstadoId { get; set; }
        public string EstadoDescripcion { get; set; } = "Activo";
        public DateTime? FechaCreacion { get; set; }
    }

    public class CrearParametroViewModel
    {
        [Required(ErrorMessage = "El ID del parámetro es requerido")]
        [Display(Name = "ID del Parámetro")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El valor del parámetro es requerido")]
        [Display(Name = "Valor")]
        public string Valor { get; set; } = string.Empty;

        [Display(Name = "Estado")]
        public int EstadoId { get; set; } = 1;
    }

    public class EditarParametroViewModel
    {
        [Required(ErrorMessage = "El ID del parámetro es requerido")]
        [Display(Name = "ID del Parámetro")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El valor del parámetro es requerido")]
        [Display(Name = "Valor")]
        public string Valor { get; set; } = string.Empty;

        [Display(Name = "Estado")]
        public int EstadoId { get; set; } = 1;
    }

    public class ParametroResponse
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public ParametroViewModel? Parametro { get; set; }
        public List<ParametroViewModel>? Parametros { get; set; }
    }
}