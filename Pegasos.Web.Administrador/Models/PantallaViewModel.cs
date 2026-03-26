namespace Pegasos.Web.Administrador.Models
{
    public class PantallaViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;
        public int Estado { get; set; }
    }

    public class CrearPantallaViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;
    }

    public class EditarPantallaViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;
        public int Estado { get; set; } = 1;
    }

    public class PantallaResponse
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public PantallaViewModel? Pantalla { get; set; }
        public List<PantallaViewModel>? Pantallas { get; set; }
    }
}