using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    // Request/Response base
    public class UsuarioRequest
    {
        public int ID_Usuario { get; set; }
        public string Nombre_Completo { get; set; }
        public string Tipo_Identificacion { get; set; }
        public string Identificacion { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Usuario { get; set; }
        public string Contraseña { get; set; }
        public int ID_Estado { get; set; }
        public int ID_Rol { get; set; }
    }

    public class UsuarioResponse
    {
        public int ID_Usuario { get; set; }
        public string Nombre_Completo { get; set; }
        public string Tipo_Identificacion { get; set; }
        public string Identificacion { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Usuario { get; set; }
        public int ID_Estado { get; set; }
        public int ID_Rol { get; set; }
    }

    // Filtros específicos SRV1
    public class UsuarioFiltroRequest
    {
        public string Identificacion { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
    }
}
