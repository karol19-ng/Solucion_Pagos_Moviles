using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class AfiliacionRequest
    {
        public string Numero_Cuenta { get; set; }
        public string Identificacion { get; set; }
        public string Numero_Telefono { get; set; }
    }

    public class AfiliacionResponse
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; }
    }
}
