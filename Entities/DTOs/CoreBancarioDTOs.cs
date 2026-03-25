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
        public string Identificacion { get; set; } = null!;
    }

    public class ClienteExisteResponse
    {
        public bool Existe { get; set; }
    }

    // Respuesta genérica para operaciones core
    public class CoreOperacionResponse
    {
        public int codigo { get; set; }
        public string descripcion { get; set; } = null!;
        public decimal? saldo { get; set; }
    }

    // DTO para listar/obtener clientes
    public class ClienteDTO
    {
        public int Id { get; set; }
        public string TipoIdentificacion { get; set; } = null!;
        public string Identificacion { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public DateTime FechaNacimiento { get; set; }
        public int? EstadoId { get; set; }
        public string EstadoDescripcion { get; set; } = null!;
    }

    // DTO para crear un cliente
    public class CrearClienteRequest
    {
        public string TipoIdentificacion { get; set; } = null!;
        public string Identificacion { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public DateTime Fecha_Nacimiento { get; set; }
    }

    // DTO para actualizar un cliente
    public class ActualizarClienteRequest
    {
        public int Id { get; set; }
        public string TipoIdentificacion { get; set; } = null!;
        public string Identificacion { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public DateTime Fecha_Nacimiento { get; set; }
        public int? EstadoId { get; set; }
    }

    // DTO para respuesta de operaciones con cliente
    public class ClienteResponse
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = null!;
        public ClienteDTO Cliente { get; set; } = null!;
        public List<ClienteDTO> Clientes { get; set; } = null!;
    }

}
