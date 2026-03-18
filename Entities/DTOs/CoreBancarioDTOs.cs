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

    // DTO para listar/obtener clientes
    public class ClienteDTO
    {
        public int Id { get; set; }
        public string TipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public string NombreCompleto { get; set; }
        public int? EstadoId { get; set; }
        public string EstadoDescripcion { get; set; }
    }

    // DTO para crear un cliente
    public class CrearClienteRequest
    {
        public string TipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public string NombreCompleto { get; set; }
    }

    // DTO para actualizar un cliente
    public class ActualizarClienteRequest
    {
        public int Id { get; set; }
        public string TipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public string NombreCompleto { get; set; }
        public int? EstadoId { get; set; }
    }

    // DTO para respuesta de operaciones con cliente
    public class ClienteResponse
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; }
        public ClienteDTO Cliente { get; set; }
        public List<ClienteDTO> Clientes { get; set; }
    }

}
