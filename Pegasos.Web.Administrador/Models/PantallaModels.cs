//using System.ComponentModel.DataAnnotations;

//namespace Pegasos.Web.Administrador.Models
//{
//    public class PantallaRequest
//    {
//        public int ID_Pantalla { get; set; }

//        [Required(ErrorMessage = "El nombre es obligatorio")]
//        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
//        public string Nombre { get; set; }

//        [StringLength(255, ErrorMessage = "La descripción no puede exceder los 255 caracteres")]
//        public string Descripcion { get; set; }

//        [Required(ErrorMessage = "La ruta es obligatoria")]
//        [StringLength(200, ErrorMessage = "La ruta no puede exceder los 200 caracteres")]
//        public string Ruta { get; set; }
//    }

//    public class PantallaResponse
//    {
//        public int ID_Pantalla { get; set; }
//        public string Nombre { get; set; }
//        public string Descripcion { get; set; }
//        public string Ruta { get; set; }
//    }

//    // ViewModel para la vista
//    public class PantallaViewModel
//    {
//        public List<PantallaResponse> Pantallas { get; set; } = new();
//        public string FiltroNombre { get; set; } = "";
//        public PantallaRequest PantallaActual { get; set; } = new();
//        public bool MostrarModal { get; set; }
//        public bool MostrarModalEliminar { get; set; }
//        public int? PantallaAEliminar { get; set; }
//        public string MensajeError { get; set; }
//        public string MensajeExito { get; set; }
//    }
//}