using System.ComponentModel.DataAnnotations;

namespace Pegasos.Web.Administrador.Models
{
    public class RolViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;  
        public List<PantallaAsignadaViewModel> Pantallas { get; set; } = new List<PantallaAsignadaViewModel>();
    }

    public class PantallaAsignadaViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;  
        public bool Asignada { get; set; }
    }

    public class CrearRolViewModel
    {
        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [Display(Name = "Nombre del Rol")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;  

        public List<int> PantallasSeleccionadas { get; set; } = new List<int>();
    }

    public class EditarRolViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [Display(Name = "Nombre del Rol")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        public List<int> PantallasSeleccionadas { get; set; } = new List<int>();
    }

    public class RolResponse
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public RolViewModel? Rol { get; set; }
        public List<RolViewModel>? Roles { get; set; }
    }
}