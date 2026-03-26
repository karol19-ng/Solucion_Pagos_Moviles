using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class RolRequest
    {
        public int ID_Rol { get; set; }
        public string Nombre { get; set; } = null!;
        public List<int> Pantallas { get; set; } = new List<int>();
        public string Descripcion { get; set; } = null!;

        
    }

    public class RolResponse
    {
        public int ID_Rol { get; set; }
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public List<PantallaResponse> Pantallas { get; set; } = new List<PantallaResponse>();
    }
}
