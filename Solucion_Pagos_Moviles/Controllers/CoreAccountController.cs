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
    [Authorize] // Todas las operaciones requieren token
    public class CoreAccountController : ControllerBase
    {
        private readonly CoreBancarioDbContext _context;
        private readonly IBitacoraService _bitacoraService;
        private readonly ILogger<CoreAccountController> _logger;

        public CoreAccountController(
            CoreBancarioDbContext context,
            IBitacoraService bitacoraService,
            ILogger<CoreAccountController> logger)
        {
            _context = context;
            _bitacoraService = bitacoraService;
            _logger = logger;
        }

        // GET: api/coreaccount
        [HttpGet]
        public async Task<ActionResult<CuentaResponse>> ListarTodos()
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var cuentas = await (from c in _context.Cuentas
                                     join cli in _context.ClientesBanco on c.Identificacion_Cliente equals cli.ID_Cliente into clienteJoin
                                     from cliente in clienteJoin.DefaultIfEmpty()
                                     join est in _context.EstadosCore on c.ID_Estado equals est.ID_Estado into estadoJoin
                                     from estado in estadoJoin.DefaultIfEmpty()
                                     select new CuentaDTO
                                     {
                                         Id = c.ID_Cuenta,
                                         NumeroCuenta = c.Numero_Cuenta,
                                         ClienteId = c.Identificacion_Cliente,
                                         ClienteIdentificacion = cliente != null ? cliente.Identificacion : "",
                                         ClienteNombre = cliente != null ? cliente.Nombre_Completo : "",
                                         Saldo = c.Saldo,
                                         EstadoId = c.ID_Estado,
                                         EstadoDescripcion = estado != null ? estado.Descripcion : ""
                                     })
                                      .ToListAsync();

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CONSULTA",
                    Descripcion = $"El usuario consulta todas las cuentas del core. Total: {cuentas.Count} registros",
                    Servicio = "/core/account",
                    Resultado = "OK"
                });

                return Ok(new CuentaResponse
                {
                    Codigo = 0,
                    Descripcion = "Cuentas obtenidas exitosamente",
                    Cuentas = cuentas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar cuentas");

                var errorMessage = ex.Message.Length > 200 ? ex.Message.Substring(0, 200) + "..." : ex.Message;

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Accion = "ERROR",
                    Descripcion = $"Error al listar cuentas: {errorMessage}",
                    Servicio = "/core/account",
                    Resultado = "ERROR"
                });

                return StatusCode(500, new CuentaResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor"
                });
            }
        }

        // GET: api/coreaccount/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CuentaResponse>> ObtenerPorId(int id)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var cuenta = await (from c in _context.Cuentas
                                    join cli in _context.ClientesBanco on c.Identificacion_Cliente equals cli.ID_Cliente into clienteJoin
                                    from cliente in clienteJoin.DefaultIfEmpty()
                                    join est in _context.EstadosCore on c.ID_Estado equals est.ID_Estado into estadoJoin
                                    from estado in estadoJoin.DefaultIfEmpty()
                                    where c.ID_Cuenta == id
                                    select new CuentaDTO
                                    {
                                        Id = c.ID_Cuenta,
                                        NumeroCuenta = c.Numero_Cuenta,
                                        ClienteId = c.Identificacion_Cliente,
                                        ClienteIdentificacion = cliente != null ? cliente.Identificacion : "",
                                        ClienteNombre = cliente != null ? cliente.Nombre_Completo : "",
                                        Saldo = c.Saldo,
                                        EstadoId = c.ID_Estado,
                                        EstadoDescripcion = estado != null ? estado.Descripcion : ""
                                    })
                                     .FirstOrDefaultAsync();

                if (cuenta == null)
                {
                    return NotFound(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = $"Cuenta con ID {id} no encontrada"
                    });
                }

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CONSULTA",
                    Descripcion = $"El usuario consulta cuenta por ID: {id}",
                    Servicio = "/core/account",
                    Resultado = "OK"
                });

                return Ok(new CuentaResponse
                {
                    Codigo = 0,
                    Descripcion = "Cuenta obtenida exitosamente",
                    Cuenta = cuenta
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cuenta {id}");

                return StatusCode(500, new CuentaResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor"
                });
            }
        }

        // GET: api/coreaccount/numero/{numeroCuenta}
        [HttpGet("numero/{numeroCuenta}")]
        public async Task<ActionResult<CuentaResponse>> ObtenerPorNumero(string numeroCuenta)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var cuenta = await (from c in _context.Cuentas
                                    join cli in _context.ClientesBanco on c.Identificacion_Cliente equals cli.ID_Cliente into clienteJoin
                                    from cliente in clienteJoin.DefaultIfEmpty()
                                    join est in _context.EstadosCore on c.ID_Estado equals est.ID_Estado into estadoJoin
                                    from estado in estadoJoin.DefaultIfEmpty()
                                    where c.Numero_Cuenta == numeroCuenta
                                    select new CuentaDTO
                                    {
                                        Id = c.ID_Cuenta,
                                        NumeroCuenta = c.Numero_Cuenta,
                                        ClienteId = c.Identificacion_Cliente,
                                        ClienteIdentificacion = cliente != null ? cliente.Identificacion : "",
                                        ClienteNombre = cliente != null ? cliente.Nombre_Completo : "",
                                        Saldo = c.Saldo,
                                        EstadoId = c.ID_Estado,
                                        EstadoDescripcion = estado != null ? estado.Descripcion : ""
                                    })
                                     .FirstOrDefaultAsync();

                if (cuenta == null)
                {
                    return NotFound(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = $"Cuenta con número {numeroCuenta} no encontrada"
                    });
                }

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CONSULTA",
                    Descripcion = $"El usuario consulta cuenta por número: {numeroCuenta}",
                    Servicio = "/core/account",
                    Resultado = "OK"
                });

                return Ok(new CuentaResponse
                {
                    Codigo = 0,
                    Descripcion = "Cuenta obtenida exitosamente",
                    Cuenta = cuenta
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cuenta por número {numeroCuenta}");

                return StatusCode(500, new CuentaResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor"
                });
            }
        }

        // GET: api/coreaccount/cliente/{identificacionCliente}
        [HttpGet("cliente/{identificacionCliente}")]
        public async Task<ActionResult<CuentaResponse>> ObtenerPorClienteIdentificacion(int identificacionCliente)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var cuentas = await (from c in _context.Cuentas
                                     join cli in _context.ClientesBanco on c.Identificacion_Cliente equals cli.ID_Cliente into clienteJoin
                                     from cliente in clienteJoin.DefaultIfEmpty()
                                     join est in _context.EstadosCore on c.ID_Estado equals est.ID_Estado into estadoJoin
                                     from estado in estadoJoin.DefaultIfEmpty()
                                     where c.Identificacion_Cliente == identificacionCliente
                                     select new CuentaDTO
                                     {
                                         Id = c.ID_Cuenta,
                                         NumeroCuenta = c.Numero_Cuenta,
                                         ClienteId = c.Identificacion_Cliente,
                                         ClienteIdentificacion = cliente != null ? cliente.Identificacion : "",
                                         ClienteNombre = cliente != null ? cliente.Nombre_Completo : "",
                                         Saldo = c.Saldo,
                                         EstadoId = c.ID_Estado,
                                         EstadoDescripcion = estado != null ? estado.Descripcion : ""
                                     })
                                      .ToListAsync();

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CONSULTA",
                    Descripcion = $"El usuario consulta cuentas del cliente ID: {identificacionCliente}. Total: {cuentas.Count}",
                    Servicio = "/core/account",
                    Resultado = "OK"
                });

                return Ok(new CuentaResponse
                {
                    Codigo = 0,
                    Descripcion = "Cuentas obtenidas exitosamente",
                    Cuentas = cuentas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cuentas del cliente {identificacionCliente}");

                return StatusCode(500, new CuentaResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor"
                });
            }
        }

        // POST: api/coreaccount
        [HttpPost]
        public async Task<ActionResult<CuentaResponse>> Crear([FromBody] CrearCuentaRequest request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                _logger.LogInformation("=== INTENTANDO CREAR CUENTA ===");
                _logger.LogInformation("Usuario: {Usuario}", usuario);
                _logger.LogInformation("Request: {@Request}", request);

                // Validaciones
                if (request == null)
                {
                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = "Debe enviar los datos de la cuenta"
                    });
                }

                if (request.IdentificacionCliente <= 0)
                {
                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = "La identificación del cliente es requerida"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.NumeroCuenta))
                {
                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = "El número de cuenta es requerido"
                    });
                }

                // Validar que el número de cuenta no exista
                var existeNumero = await _context.Cuentas
                    .AnyAsync(c => c.Numero_Cuenta == request.NumeroCuenta);

                if (existeNumero)
                {
                    return Conflict(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = $"Ya existe una cuenta con el número {request.NumeroCuenta}"
                    });
                }

                // Validar que el cliente existe
                var cliente = await _context.ClientesBanco
                    .FirstOrDefaultAsync(c => c.ID_Cliente == request.IdentificacionCliente);

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente no encontrado: {IdentificacionCliente}", request.IdentificacionCliente);
                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = $"El cliente con ID {request.IdentificacionCliente} no existe"
                    });
                }

                // Obtener el máximo ID actual
                var maxId = await _context.Cuentas
                    .OrderByDescending(c => c.ID_Cuenta)
                    .Select(c => (int?)c.ID_Cuenta)
                    .FirstOrDefaultAsync();

                int nuevoId = maxId.HasValue ? maxId.Value + 1 : 1;

                _logger.LogInformation("Máximo ID actual: {MaxId}, Nuevo ID asignado: {NuevoId}", maxId, nuevoId);

                // Crear nueva cuenta
                var nuevaCuenta = new Cuenta
                {
                    ID_Cuenta = nuevoId,
                    Numero_Cuenta = request.NumeroCuenta,
                    Identificacion_Cliente = request.IdentificacionCliente,
                    Saldo = 0,
                    ID_Estado = 1 // Activo
                };

                _logger.LogInformation("Cuenta a insertar: {@Cuenta}", new
                {
                    nuevaCuenta.ID_Cuenta,
                    nuevaCuenta.Numero_Cuenta,
                    nuevaCuenta.Identificacion_Cliente,
                    nuevaCuenta.Saldo,
                    nuevaCuenta.ID_Estado
                });

                _context.Cuentas.Add(nuevaCuenta);
                var saveResult = await _context.SaveChangesAsync();
                _logger.LogInformation("SaveChangesAsync result: {SaveResult}", saveResult);

                // Registrar en bitácora
                var jsonRegistro = System.Text.Json.JsonSerializer.Serialize(new
                {
                    nuevaCuenta.ID_Cuenta,
                    nuevaCuenta.Numero_Cuenta,
                    nuevaCuenta.Identificacion_Cliente,
                    nuevaCuenta.Saldo,
                    nuevaCuenta.ID_Estado
                });

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CREACION",
                    Descripcion = $"Nueva cuenta creada: {jsonRegistro}",
                    Servicio = "/core/account",
                    Resultado = "OK"
                });

                var cuentaDto = new CuentaDTO
                {
                    Id = nuevaCuenta.ID_Cuenta,
                    NumeroCuenta = nuevaCuenta.Numero_Cuenta,
                    ClienteId = nuevaCuenta.Identificacion_Cliente,
                    ClienteIdentificacion = cliente.Identificacion,
                    ClienteNombre = cliente.Nombre_Completo,
                    Saldo = nuevaCuenta.Saldo,
                    EstadoId = nuevaCuenta.ID_Estado
                };

                _logger.LogInformation("Cuenta creada exitosamente con ID: {Id}", nuevaCuenta.ID_Cuenta);

                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevaCuenta.ID_Cuenta }, new CuentaResponse
                {
                    Codigo = 0,
                    Descripcion = "Cuenta creada exitosamente",
                    Cuenta = cuentaDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuenta");
                _logger.LogError("Mensaje: {Message}", ex.Message);
                _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    _logger.LogError("InnerException: {InnerMessage}", ex.InnerException.Message);
                }

                return StatusCode(500, new CuentaResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor: " + ex.Message
                });
            }
        }

        // PUT: api/coreaccount/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<CuentaResponse>> Actualizar(int id, [FromBody] ActualizarCuentaRequest request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                // Validaciones
                if (request == null)
                {
                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = "Debe enviar los datos de la cuenta"
                    });
                }

                if (id != request.Id)
                {
                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = "El ID de la cuenta no coincide"
                    });
                }

                // Buscar cuenta existente
                var cuentaExistente = await _context.Cuentas
                    .FirstOrDefaultAsync(c => c.ID_Cuenta == id);

                if (cuentaExistente == null)
                {
                    return NotFound(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = $"Cuenta con ID {id} no encontrada"
                    });
                }

                // Guardar copia para bitácora
                var cuentaAnterior = new
                {
                    cuentaExistente.Numero_Cuenta,
                    cuentaExistente.Identificacion_Cliente,
                    cuentaExistente.Saldo,
                    cuentaExistente.ID_Estado
                };

                // Actualizar estado si se envía
                if (request.EstadoId.HasValue)
                {
                    // Validar que el estado existe
                    var estadoValido = await _context.EstadosCore.AnyAsync(e => e.ID_Estado == request.EstadoId.Value);
                    if (!estadoValido)
                    {
                        return BadRequest(new CuentaResponse
                        {
                            Codigo = -1,
                            Descripcion = $"El estado con ID {request.EstadoId} no existe"
                        });
                    }
                    cuentaExistente.ID_Estado = request.EstadoId.Value;
                }

                // Actualizar saldo si se envía
                if (request.Saldo.HasValue)
                {
                    cuentaExistente.Saldo = request.Saldo.Value;
                }

                await _context.SaveChangesAsync();

                // Obtener datos del cliente para el DTO
                var cliente = await _context.ClientesBanco
                    .FirstOrDefaultAsync(c => c.ID_Cliente == cuentaExistente.Identificacion_Cliente);

                // Registrar en bitácora
                var jsonAnterior = System.Text.Json.JsonSerializer.Serialize(cuentaAnterior);
                var jsonActual = System.Text.Json.JsonSerializer.Serialize(new
                {
                    cuentaExistente.Numero_Cuenta,
                    cuentaExistente.Identificacion_Cliente,
                    cuentaExistente.Saldo,
                    cuentaExistente.ID_Estado
                });

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "ACTUALIZACION",
                    Descripcion = $"Cuenta actualizada - Anterior: {jsonAnterior}, Actual: {jsonActual}",
                    Servicio = "/core/account",
                    Resultado = "OK"
                });

                var cuentaDto = new CuentaDTO
                {
                    Id = cuentaExistente.ID_Cuenta,
                    NumeroCuenta = cuentaExistente.Numero_Cuenta,
                    ClienteId = cuentaExistente.Identificacion_Cliente,
                    ClienteIdentificacion = cliente?.Identificacion ?? "",
                    ClienteNombre = cliente?.Nombre_Completo ?? "",
                    Saldo = cuentaExistente.Saldo,
                    EstadoId = cuentaExistente.ID_Estado
                };

                return Ok(new CuentaResponse
                {
                    Codigo = 0,
                    Descripcion = "Cuenta actualizada exitosamente",
                    Cuenta = cuentaDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar cuenta {id}");

                return StatusCode(500, new CuentaResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor"
                });
            }
        }

        // DELETE: api/coreaccount/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<CuentaResponse>> Eliminar(int id)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                _logger.LogInformation("=== INTENTANDO ELIMINAR CUENTA ===");
                _logger.LogInformation("ID a eliminar: {Id}", id);
                _logger.LogInformation("Usuario: {Usuario}", usuario);

                // Buscar cuenta existente
                var cuentaExistente = await _context.Cuentas
                    .FirstOrDefaultAsync(c => c.ID_Cuenta == id);

                if (cuentaExistente == null)
                {
                    _logger.LogWarning("Cuenta con ID {Id} no encontrada", id);
                    return NotFound(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = $"Cuenta con ID {id} no encontrada"
                    });
                }

                _logger.LogInformation("Cuenta encontrada: {@Cuenta}", new
                {
                    cuentaExistente.ID_Cuenta,
                    cuentaExistente.Numero_Cuenta,
                    cuentaExistente.Identificacion_Cliente,
                    cuentaExistente.Saldo,
                    cuentaExistente.ID_Estado
                });

                // Verificar si tiene movimientos asociados usando MovimientoId
                var tieneMovimientos = await _context.MovimientosCuenta
                    .AnyAsync(m => m.Numero_Cuenta == cuentaExistente.Numero_Cuenta);

                if (tieneMovimientos)
                {
                    _logger.LogWarning("Cuenta {NumeroCuenta} tiene movimientos asociados. No se puede eliminar.", cuentaExistente.Numero_Cuenta);

                    var cantidadMovimientos = await _context.MovimientosCuenta
                        .CountAsync(m => m.Numero_Cuenta == cuentaExistente.Numero_Cuenta);

                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = $"No se puede eliminar la cuenta porque tiene {cantidadMovimientos} movimiento(s) asociado(s)"
                    });
                }

                // Guardar copia para bitácora
                var cuentaEliminada = new
                {
                    cuentaExistente.ID_Cuenta,
                    cuentaExistente.Numero_Cuenta,
                    cuentaExistente.Identificacion_Cliente,
                    cuentaExistente.Saldo,
                    cuentaExistente.ID_Estado
                };

                // Eliminar cuenta
                _context.Cuentas.Remove(cuentaExistente);
                var saveResult = await _context.SaveChangesAsync();

                _logger.LogInformation("SaveChangesAsync result: {SaveResult}", saveResult);

                // Registrar en bitácora
                var jsonEliminado = System.Text.Json.JsonSerializer.Serialize(cuentaEliminada);
                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "ELIMINACION",
                    Descripcion = $"Cuenta eliminada: {jsonEliminado}",
                    Servicio = "/core/account",
                    Resultado = "OK"
                });

                _logger.LogInformation("Cuenta {Id} eliminada exitosamente", id);

                return Ok(new CuentaResponse
                {
                    Codigo = 0,
                    Descripcion = "Cuenta eliminada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar cuenta {id}");
                _logger.LogError("Mensaje: {Message}", ex.Message);
                _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    _logger.LogError("InnerException: {InnerMessage}", ex.InnerException.Message);
                }

                return StatusCode(500, new CuentaResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor: " + ex.Message
                });
            }
        }
    }

    // DTOs para las respuestas
    public class CuentaDTO
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; } = "";
        public int ClienteId { get; set; }
        public string ClienteIdentificacion { get; set; } = "";
        public string ClienteNombre { get; set; } = "";
        public decimal Saldo { get; set; }
        public int? EstadoId { get; set; }
        public string EstadoDescripcion { get; set; } = "";
    }

    public class CuentaResponse
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = "";
        public CuentaDTO? Cuenta { get; set; }
        public List<CuentaDTO>? Cuentas { get; set; }
    }

    public class CrearCuentaRequest
    {
        public int IdentificacionCliente { get; set; }
        public string NumeroCuenta { get; set; } = "";
    }

    public class ActualizarCuentaRequest
    {
        public int Id { get; set; }
        public decimal? Saldo { get; set; }
        public int? EstadoId { get; set; }
    }
}