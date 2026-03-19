using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class PantallaRequest
    {
        public int ID_Pantalla { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Ruta { get; set; }

        public int Estado { get; set; }
    }

    public class PantallaResponse
    {
        public int ID_Pantalla { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Ruta { get; set; }
    }
}
