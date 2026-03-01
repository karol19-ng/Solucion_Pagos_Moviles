using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    // SRV19 - Verificar cliente
    public class ClienteExisteRequest
    {
        public string Identificacion { get; set; }
    }

    public class ClienteExisteResponse
    {
        public bool Existe { get; set; }
    }

    // Respuesta genérica para operaciones core
    public class CoreOperacionResponse
    {
        public int codigo { get; set; }
        public string descripcion { get; set; }
        public decimal? saldo { get; set; }
    }
}
