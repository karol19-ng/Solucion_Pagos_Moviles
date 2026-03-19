namespace Pegasos.Web.Administrador.Models
{
    public class RolViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public List<PantallaAsignadaViewModel> Pantallas { get; set; } = new List<PantallaAsignadaViewModel>();
    }

    public class PantallaAsignadaViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Asignada { get; set; }
    }

    public class CrearRolViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public List<int> PantallasSeleccionadas { get; set; } = new List<int>();
    }

    public class EditarRolViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
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