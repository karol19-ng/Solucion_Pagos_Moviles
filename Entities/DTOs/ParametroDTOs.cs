using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class ParametroRequest
    {
        public string ID_Parametro { get; set; }
        public string Valor { get; set; }
    }

    public class ParametroResponse
    {
        public string ID_Parametro { get; set; }
        public string Valor { get; set; }
    }
}
