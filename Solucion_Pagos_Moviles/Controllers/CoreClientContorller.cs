using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractDataAccess.Models;
using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Solucion_Pagos_Moviles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class CoreClientController : ControllerBase
    {
        private readonly CoreBancarioDbContext _context;
        private readonly IBitacoraService _bitacoraService;
        private readonly ILogger<CoreClientController> _logger;

        public CoreClientController(
            CoreBancarioDbContext context,
            IBitacoraService bitacoraService,
            ILogger<CoreClientController> logger)
        {
            _context = context;
            _bitacoraService = bitacoraService;
            _logger = logger;
        }

        // GET: api/coreclient
        [HttpGet]
        public async Task<ActionResult<ClienteResponse>> ListarTodos()
        {
            try
            {
                _logger.LogInformation("=== LISTANDO CLIENTES ===");

                // Probar una consulta simple primero
                var count = await _context.ClientesBanco.CountAsync();
                _logger.LogInformation("Total clientes en BD: {Count}", count);

                // Obtener un cliente de prueba sin usar el DTO
                var primerCliente = await _context.ClientesBanco.FirstOrDefaultAsync();
                if (primerCliente != null)
                {
                    _logger.LogInformation("Primer cliente - ID: {Id}, Nombre: {Nombre}",
                        primerCliente.ID_Cliente, primerCliente.Nombre_Completo);

                    // Intentar acceder a Telefono y FechaNacimiento
                    try
                    {
                        var telefono = primerCliente.Telefono;
                        var fechaNac = primerCliente.Fecha_Nacimiento;
                        _logger.LogInformation("Telefono: {Telefono}, FechaNac: {FechaNac}", telefono, fechaNac);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al acceder a Telefono/FechaNacimiento");
                    }
                }

                // Intentar la consulta completa
                var clientes = new List<ClienteDTO>();

                foreach (var c in await _context.ClientesBanco.ToListAsync())
                {
                    clientes.Add(new ClienteDTO
                    {
                        Id = c.ID_Cliente,
                        TipoIdentificacion = c.Tipo_Identificacion,
                        Identificacion = c.Identificacion,
                        NombreCompleto = c.Nombre_Completo,
                        Telefono = c.Telefono,
                        FechaNacimiento = c.Fecha_Nacimiento,
                        EstadoId = c.ID_Estado
                    });
                }

                _logger.LogInformation("Clientes procesados: {Count}", clientes.Count);

                return Ok(new ClienteResponse
                {
                    Codigo = 0,
                    Descripcion = "Clientes obtenidos exitosamente",
                    Clientes = clientes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR AL LISTAR CLIENTES");
                _logger.LogError("Message: {Message}", ex.Message);
                _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    _logger.LogError("InnerException: {InnerMessage}", ex.InnerException.Message);
                }

                // Devolver el error detallado temporalmente para debug
                return StatusCode(500, new ClienteResponse
                {
                    Codigo = -1,
                    Descripcion = $"Error: {ex.Message}"
                });
            }
        }

        // GET: api/coreclient/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteResponse>> ObtenerPorId(int id)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var cliente = await _context.ClientesBanco
                    .Where(c => c.ID_Cliente == id)
                    .Select(c => new ClienteDTO
                    {
                        Id = c.ID_Cliente,
                        TipoIdentificacion = c.Tipo_Identificacion,
                        Identificacion = c.Identificacion,
                        NombreCompleto = c.Nombre_Completo,
                        Telefono = c.Telefono,                    
                        FechaNacimiento = c.Fecha_Nacimiento,     
                        EstadoId = c.ID_Estado
                    })
                    .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    return NotFound(new ClienteResponse
                    {
                        Codigo = -1,
                        Descripcion = $"Cliente con ID {id} no encontrado"
                    });
                }

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CONSULTA",
                    Descripcion = $"El usuario consulta cliente por ID: {id}",
                    Servicio = "/core/client",
                    Resultado = "OK"
                });

                return Ok(new ClienteResponse
                {
                    Codigo = 0,
                    Descripcion = "Cliente obtenido exitosamente",
                    Cliente = cliente
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cliente {id}");
                return StatusCode(500, new ClienteResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor"
                });
            }
        }

        // GET: api/coreclient/identificacion/{identificacion}
        [HttpGet("identificacion/{identificacion}")]
        public async Task<ActionResult<ClienteResponse>> ObtenerPorIdentificacion(string identificacion)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var cliente = await _context.ClientesBanco
                    .Where(c => c.Identificacion == identificacion)
                    .Select(c => new ClienteDTO
                    {
                        Id = c.ID_Cliente,
                        TipoIdentificacion = c.Tipo_Identificacion,
                        Identificacion = c.Identificacion,
                        NombreCompleto = c.Nombre_Completo,
                        EstadoId = c.ID_Estado
                    })
                    .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    return NotFound(new ClienteResponse
                    {
                        Codigo = -1,
                        Descripcion = $"Cliente con identificación {identificacion} no encontrado"
                    });
                }

                
                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CONSULTA",
                    Descripcion = $"El usuario consulta cliente por identificación: {identificacion}",
                    Servicio = "/core/client",
                    Resultado = "OK"
                });

                return Ok(new ClienteResponse
                {
                    Codigo = 0,
                    Descripcion = "Cliente obtenido exitosamente",
                    Cliente = cliente
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cliente por identificación {identificacion}");

                return StatusCode(500, new ClienteResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor"
                });
            }
        }


        // POST: api/coreclient
        [HttpPost]
        public async Task<ActionResult<ClienteResponse>> Crear([FromBody] CrearClienteRequest request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                _logger.LogInformation("=== INTENTANDO CREAR CLIENTE (Solución 2 - ID Manual) ===");
                _logger.LogInformation("Usuario: {Usuario}", usuario);
                _logger.LogInformation("Request: {@Request}", request);

                // Validaciones
                if (request == null)
                {
                    return BadRequest(new ClienteResponse
                    {
                        Codigo = -1,
                        Descripcion = "Debe enviar los datos del cliente"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Identificacion))
                {
                    return BadRequest(new ClienteResponse
                    {
                        Codigo = -1,
                        Descripcion = "La identificación es requerida"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.NombreCompleto))
                {
                    return BadRequest(new ClienteResponse
                    {
                        Codigo = -1,
                        Descripcion = "El nombre completo es requerido"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.TipoIdentificacion))
                {
                    return BadRequest(new ClienteResponse
                    {
                        Codigo = -1,
                        Descripcion = "El tipo de identificación es requerido"
                    });
                }

                // Validar que no exista un cliente con la misma identificación
                var existe = await _context.ClientesBanco
                    .AnyAsync(c => c.Identificacion == request.Identificacion);

                if (existe)
                {
                    _logger.LogWarning("Cliente duplicado: {Identificacion}", request.Identificacion);
                    return Conflict(new ClienteResponse
                    {
                        Codigo = -1,
                        Descripcion = $"Ya existe un cliente con la identificación {request.Identificacion}"
                    });
                }

               
                var maxId = await _context.ClientesBanco
                    .OrderByDescending(c => c.ID_Cliente)
                    .Select(c => (int?)c.ID_Cliente)
                    .FirstOrDefaultAsync();

               
                int nuevoId = maxId.HasValue ? maxId.Value + 1 : 1;

                _logger.LogInformation("Máximo ID actual: {MaxId}, Nuevo ID asignado: {NuevoId}", maxId, nuevoId);

                // Crear nuevo cliente con ID manual
                var nuevoCliente = new ClienteBanco
                {
                    ID_Cliente = nuevoId,  
                    Tipo_Identificacion = request.TipoIdentificacion,
                    Identificacion = request.Identificacion,
                    Nombre_Completo = request.NombreCompleto,
                    Telefono = request.Telefono,
                    Fecha_Nacimiento = request.Fecha_Nacimiento,
                    ID_Estado = 1 
                };

                _logger.LogInformation("Cliente a insertar: {@Cliente}", nuevoCliente);

                _context.ClientesBanco.Add(nuevoCliente);

                var saveResult = await _context.SaveChangesAsync();
                _logger.LogInformation("SaveChangesAsync result: {SaveResult}", saveResult);

                // Registrar en bitácora
                var jsonRegistro = System.Text.Json.JsonSerializer.Serialize(nuevoCliente);
                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CREACION",
                    Descripcion = $"Nuevo cliente creado: {jsonRegistro}",
                    Servicio = "/core/client",
                    Resultado = "OK"
                });

                var clienteDto = new ClienteDTO
                {
                    Id = nuevoCliente.ID_Cliente,
                    TipoIdentificacion = nuevoCliente.Tipo_Identificacion,
                    Identificacion = nuevoCliente.Identificacion,
                    NombreCompleto = nuevoCliente.Nombre_Completo,
                    EstadoId = nuevoCliente.ID_Estado
                };

                _logger.LogInformation("Cliente creado exitosamente con ID: {Id}", nuevoCliente.ID_Cliente);

                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoCliente.ID_Cliente }, new ClienteResponse
                {
                    Codigo = 0,
                    Descripcion = "Cliente creado exitosamente",
                    Cliente = clienteDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente");
                _logger.LogError("Mensaje: {Message}", ex.Message);
                _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    _logger.LogError("InnerException: {InnerMessage}", ex.InnerException.Message);
                }

                return StatusCode(500, new ClienteResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor: " + ex.Message
                });
            }
        }


        // PUT: api/coreclient/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ClienteResponse>> Actualizar(int id, [FromBody] ActualizarClienteRequest request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                // Validaciones
                if (request == null)
                {
                    return BadRequest(new ClienteResponse
                    {
                        Codigo = -1,
                        Descripcion = "Debe enviar los datos del cliente"
                    });
                }

                if (id != request.Id)
                {
                    return BadRequest(new ClienteResponse
                    {
                        Codigo = -1,
                        Descripcion = "El ID del cliente no coincide"
                    });
                }

                // Buscar cliente existente
                var clienteExistente = await _context.ClientesBanco
                    .FirstOrDefaultAsync(c => c.ID_Cliente == id);

                if (clienteExistente == null)
                {
                    return NotFound(new ClienteResponse
                    {
                        Codigo = -1,
                        Descripcion = $"Cliente con ID {id} no encontrado"
                    });
                }

                // Guardar copia para bitácora
                var clienteAnterior = new
                {
                    clienteExistente.Tipo_Identificacion,
                    clienteExistente.Identificacion,
                    clienteExistente.Nombre_Completo,
                    clienteExistente.ID_Estado
                };

                // Validar que la nueva identificación no esté en uso por otro cliente
                if (clienteExistente.Identificacion != request.Identificacion)
                {
                    var existeIdentificacion = await _context.ClientesBanco
                        .AnyAsync(c => c.Identificacion == request.Identificacion && c.ID_Cliente != id);

                    if (existeIdentificacion)
                    {
                        return Conflict(new ClienteResponse
                        {
                            Codigo = -1,
                            Descripcion = $"Ya existe otro cliente con la identificación {request.Identificacion}"
                        });
                    }
                }

                // Actualizar datos
                clienteExistente.Tipo_Identificacion = request.TipoIdentificacion;
                clienteExistente.Identificacion = request.Identificacion;
                clienteExistente.Nombre_Completo = request.NombreCompleto;
                if (request.EstadoId.HasValue)
                {
                    clienteExistente.ID_Estado = request.EstadoId.Value;
                }

                await _context.SaveChangesAsync();

                // Registrar en bitácora
                var jsonAnterior = System.Text.Json.JsonSerializer.Serialize(clienteAnterior);
                var jsonActual = System.Text.Json.JsonSerializer.Serialize(new
                {
                    clienteExistente.Tipo_Identificacion,
                    clienteExistente.Identificacion,
                    clienteExistente.Nombre_Completo,
                    clienteExistente.ID_Estado
                });

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "ACTUALIZACION",
                    Descripcion = $"Cliente actualizado - Anterior: {jsonAnterior}, Actual: {jsonActual}",
                    Servicio = "/core/client",
                    Resultado = "OK"
                });

                var clienteDto = new ClienteDTO
                {
                    Id = clienteExistente.ID_Cliente,
                    TipoIdentificacion = clienteExistente.Tipo_Identificacion,
                    Identificacion = clienteExistente.Identificacion,
                    NombreCompleto = clienteExistente.Nombre_Completo,
                    Telefono = request.Telefono,
                    FechaNacimiento = request.Fecha_Nacimiento,
                    EstadoId = clienteExistente.ID_Estado
                };

                return Ok(new ClienteResponse
                {
                    Codigo = 0,
                    Descripcion = "Cliente actualizado exitosamente",
                    Cliente = clienteDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar cliente {id}");

                return StatusCode(500, new ClienteResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor"
                });
            }
        }

        // DELETE: api/coreclient/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ClienteResponse>> Eliminar(int id)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                _logger.LogInformation("=== INTENTANDO ELIMINAR CLIENTE ===");
                _logger.LogInformation("ID a eliminar: {Id}", id);
                _logger.LogInformation("Usuario: {Usuario}", usuario);

               
                var clienteExistente = await _context.ClientesBanco
                    .FirstOrDefaultAsync(c => c.ID_Cliente == id);

                if (clienteExistente == null)
                {
                    _logger.LogWarning("Cliente con ID {Id} no encontrado", id);
                    return NotFound(new ClienteResponse
                    {
                        Codigo = -1,
                        Descripcion = $"Cliente con ID {id} no encontrado"
                    });
                }

                _logger.LogInformation("Cliente encontrado: {@Cliente}", clienteExistente);

                
                var tieneCuentas = await _context.Cuentas
                    .AnyAsync(c => c.Identificacion_Cliente == id);

                if (tieneCuentas)
                {
                    _logger.LogWarning("Cliente {Id} tiene cuentas asociadas. No se puede eliminar.", id);

                    // Contar cuántas cuentas tiene
                    var cantidadCuentas = await _context.Cuentas
                        .CountAsync(c => c.Identificacion_Cliente == id);

                    return BadRequest(new ClienteResponse
                    {
                        Codigo = -1,
                        Descripcion = $"No se puede eliminar el cliente porque tiene {cantidadCuentas} cuenta(s) asociada(s)"
                    });
                }

                // Guardar copia para bitácora
                var clienteEliminado = new
                {
                    clienteExistente.ID_Cliente,
                    clienteExistente.Tipo_Identificacion,
                    clienteExistente.Identificacion,
                    clienteExistente.Nombre_Completo,
                    clienteExistente.ID_Estado
                };

                // Eliminar cliente
                _context.ClientesBanco.Remove(clienteExistente);
                var saveResult = await _context.SaveChangesAsync();

                _logger.LogInformation("SaveChangesAsync result: {SaveResult}", saveResult);

                // Registrar en bitácora
                var jsonEliminado = System.Text.Json.JsonSerializer.Serialize(clienteEliminado);
                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "ELIMINACION",
                    Descripcion = $"Cliente eliminado: {jsonEliminado}",
                    Servicio = "/core/client",
                    Resultado = "OK"
                });

                _logger.LogInformation("Cliente {Id} eliminado exitosamente", id);

                return Ok(new ClienteResponse
                {
                    Codigo = 0,
                    Descripcion = "Cliente eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar cliente {id}");
                _logger.LogError("Mensaje: {Message}", ex.Message);
                _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    _logger.LogError("InnerException: {InnerMessage}", ex.InnerException.Message);
                }

                return StatusCode(500, new ClienteResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor: " + ex.Message
                });
            }
        }

        // GET: api/coreclient/tipos-identificacion
        [HttpGet("tipos-identificacion")]
        public async Task<ActionResult<List<string>>> ObtenerTiposIdentificacion()
        {
            try
            {
                // Obtener tipos de identificación únicos de la base de datos
                var tipos = await _context.ClientesBanco
                    .Select(c => c.Tipo_Identificacion)
                    .Distinct()
                    .ToListAsync();

                // Si no hay tipos, agregar algunos por defecto
                if (tipos.Count == 0)
                {
                    tipos = new List<string> { "FISICA", "JURIDICA", "DIMEX", "NITE" };
                }

                return Ok(tipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de identificación");
                return StatusCode(500, new List<string> { "FISICA", "JURIDICA", "DIMEX", "NITE" });
            }
        }
    }
}