using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class EntidadRequest
    {
        public int ID_Entidad { get; set; }
        public string Nombre_Institucion { get; set; }
    }

    public class EntidadResponse
    {
        public int ID_Entidad { get; set; }
        public string Nombre_Institucion { get; set; }
    }
}
