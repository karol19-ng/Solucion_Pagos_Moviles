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

        // GET: api/coreaccount/tipos-cuenta
        [HttpGet("tipos-cuenta")]
        public async Task<ActionResult<List<string>>> GetTiposCuenta()
        {
            try
            {
                _logger.LogInformation("Endpoint tipos-cuenta llamado");
                var tipos = new List<string> { "CORRIENTE", "AHORROS" };

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = User.Identity?.Name ?? "Sistema",
                    Accion = "CONSULTA",
                    Descripcion = "Consulta de tipos de cuenta",
                    Servicio = "/core/account/tipos-cuenta",
                    Resultado = "OK"
                });

                return Ok(tipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de cuenta");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
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
                                         TipoCuenta = c.Tipo_Cuenta ?? "AHORROS",
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
                                        TipoCuenta = c.Tipo_Cuenta ?? "AHORROS",
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
                                        TipoCuenta = c.Tipo_Cuenta ?? "AHORROS",
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
        public async Task<ActionResult<CuentaResponse>> ObtenerPorClienteIdentificacion(string identificacionCliente)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var cliente = await _context.ClientesBanco
                    .FirstOrDefaultAsync(c => c.Identificacion == identificacionCliente);

                if (cliente == null)
                {
                    return Ok(new CuentaResponse
                    {
                        Codigo = 0,
                        Descripcion = $"No se encontró cliente con identificación {identificacionCliente}",
                        Cuentas = new List<CuentaDTO>()
                    });
                }

                var cuentas = await (from c in _context.Cuentas
                                     join cli in _context.ClientesBanco on c.Identificacion_Cliente equals cli.ID_Cliente into clienteJoin
                                     from cliData in clienteJoin.DefaultIfEmpty()
                                     join est in _context.EstadosCore on c.ID_Estado equals est.ID_Estado into estadoJoin
                                     from estado in estadoJoin.DefaultIfEmpty()
                                     where c.Identificacion_Cliente == cliente.ID_Cliente
                                     select new CuentaDTO
                                     {
                                         Id = c.ID_Cuenta,
                                         NumeroCuenta = c.Numero_Cuenta,
                                         TipoCuenta = c.Tipo_Cuenta ?? "AHORROS",
                                         ClienteId = c.Identificacion_Cliente,
                                         ClienteIdentificacion = cliData != null ? cliData.Identificacion : "",
                                         ClienteNombre = cliData != null ? cliData.Nombre_Completo : "",
                                         Saldo = c.Saldo,
                                         EstadoId = c.ID_Estado,
                                         EstadoDescripcion = estado != null ? estado.Descripcion : ""
                                     })
                                      .ToListAsync();

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

        // GET: api/coreaccount/cliente-id/{clienteId}
        [HttpGet("cliente-id/{clienteId}")]
        public async Task<ActionResult<CuentaResponse>> ObtenerPorClienteId(int clienteId)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var cuentas = await (from c in _context.Cuentas
                                     join cli in _context.ClientesBanco on c.Identificacion_Cliente equals cli.ID_Cliente into clienteJoin
                                     from cliente in clienteJoin.DefaultIfEmpty()
                                     join est in _context.EstadosCore on c.ID_Estado equals est.ID_Estado into estadoJoin
                                     from estado in estadoJoin.DefaultIfEmpty()
                                     where c.Identificacion_Cliente == clienteId
                                     select new CuentaDTO
                                     {
                                         Id = c.ID_Cuenta,
                                         NumeroCuenta = c.Numero_Cuenta,
                                         TipoCuenta = c.Tipo_Cuenta ?? "AHORROS",
                                         ClienteId = c.Identificacion_Cliente,
                                         ClienteIdentificacion = cliente != null ? cliente.Identificacion : "",
                                         ClienteNombre = cliente != null ? cliente.Nombre_Completo : "",
                                         Saldo = c.Saldo,
                                         EstadoId = c.ID_Estado,
                                         EstadoDescripcion = estado != null ? estado.Descripcion : ""
                                     })
                                      .ToListAsync();

                return Ok(new CuentaResponse
                {
                    Codigo = 0,
                    Descripcion = "Cuentas obtenidas exitosamente",
                    Cuentas = cuentas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cuentas del cliente ID {clienteId}");
                return StatusCode(500, new CuentaResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor"
                });
            }
        }


        // POST: api/coreaccount - CREAR CUENTA
        [HttpPost]
        public async Task<ActionResult<CuentaResponse>> Crear([FromBody] CrearCuentaRequest request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                _logger.LogInformation("=== INTENTANDO CREAR CUENTA ===");
                _logger.LogInformation("Usuario: {Usuario}", usuario);
                _logger.LogInformation("Request: ClienteId={ClienteId}, TipoCuenta={TipoCuenta}",
                    request?.ClienteId, request?.TipoCuenta);

                // Validaciones
                if (request == null)
                {
                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = "Debe enviar los datos de la cuenta"
                    });
                }

                if (request.ClienteId <= 0)
                {
                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = "El ID del cliente es requerido"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.TipoCuenta))
                {
                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = "El tipo de cuenta es requerido"
                    });
                }

                // Validar que el tipo de cuenta sea válido
                var tiposValidos = new List<string> { "CORRIENTE", "AHORROS" };
                if (!tiposValidos.Contains(request.TipoCuenta.ToUpper()))
                {
                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = $"Tipo de cuenta inválido. Los tipos válidos son: {string.Join(", ", tiposValidos)}"
                    });
                }

                // Validar que el cliente existe
                var cliente = await _context.ClientesBanco
                    .FirstOrDefaultAsync(c => c.ID_Cliente == request.ClienteId);

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente no encontrado: {ClienteId}", request.ClienteId);
                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = $"El cliente con ID {request.ClienteId} no existe"
                    });
                }

                _logger.LogInformation("Cliente encontrado: ID={Id}, Nombre={Nombre}, Identificacion={Identificacion}",
                    cliente.ID_Cliente, cliente.Nombre_Completo, cliente.Identificacion);

                // Generar número de cuenta (sin usar ID, porque la BD lo genera automáticamente)
                string numeroCuenta = GenerarNumeroCuenta(cliente.ID_Cliente, request.TipoCuenta);
                _logger.LogInformation("Número de cuenta generado: {NumeroCuenta}", numeroCuenta);

                // Verificar que el número de cuenta no exista
                var existeNumero = await _context.Cuentas
                    .AnyAsync(c => c.Numero_Cuenta == numeroCuenta);

                if (existeNumero)
                {
                    _logger.LogWarning("Número de cuenta {NumeroCuenta} ya existe, generando alternativo", numeroCuenta);
                    numeroCuenta = GenerarNumeroCuentaAlternativo(cliente.ID_Cliente, request.TipoCuenta);
                    _logger.LogInformation("Número alternativo generado: {NumeroCuenta}", numeroCuenta);
                }

                // Crear nueva cuenta (SIN ESPECIFICAR ID_Cuenta - la BD lo genera automáticamente)
                var nuevaCuenta = new Cuenta
                {
                    Numero_Cuenta = numeroCuenta,
                    Tipo_Cuenta = request.TipoCuenta.ToUpper(),
                    Identificacion_Cliente = request.ClienteId,
                    Saldo = 0,
                    ID_Estado = 1,
                };

                _logger.LogInformation("Cuenta a insertar: Numero={Numero}, Tipo={Tipo}, ClienteId={ClienteId}, Saldo={Saldo}, Estado={Estado}",
                    nuevaCuenta.Numero_Cuenta, nuevaCuenta.Tipo_Cuenta,
                    nuevaCuenta.Identificacion_Cliente, nuevaCuenta.Saldo, nuevaCuenta.ID_Estado);

                _context.Cuentas.Add(nuevaCuenta);

                try
                {
                    var saveResult = await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ SaveChangesAsync exitoso: {SaveResult} registros afectados", saveResult);
                    _logger.LogInformation("Nuevo ID generado automáticamente: {NuevoId}", nuevaCuenta.ID_Cuenta);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "❌ DbUpdateException al guardar la cuenta");
                    _logger.LogError("Mensaje: {Message}", dbEx.Message);

                    if (dbEx.InnerException != null)
                    {
                        _logger.LogError("InnerException: {InnerMessage}", dbEx.InnerException.Message);
                        return StatusCode(500, new CuentaResponse
                        {
                            Codigo = -1,
                            Descripcion = $"Error de base de datos: {dbEx.InnerException.Message}"
                        });
                    }

                    return StatusCode(500, new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = $"Error al guardar: {dbEx.Message}"
                    });
                }

                // Registrar en bitácora
                var jsonRegistro = System.Text.Json.JsonSerializer.Serialize(new
                {
                    nuevaCuenta.ID_Cuenta,
                    nuevaCuenta.Numero_Cuenta,
                    nuevaCuenta.Tipo_Cuenta,
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
                    TipoCuenta = nuevaCuenta.Tipo_Cuenta ?? "AHORROS",
                    ClienteId = nuevaCuenta.Identificacion_Cliente,
                    ClienteIdentificacion = cliente.Identificacion,
                    ClienteNombre = cliente.Nombre_Completo,
                    Saldo = nuevaCuenta.Saldo,
                    EstadoId = nuevaCuenta.ID_Estado
                };

                _logger.LogInformation("✅ Cuenta creada exitosamente con ID: {Id}, Número: {Numero}, Tipo: {Tipo}",
                    nuevaCuenta.ID_Cuenta, nuevaCuenta.Numero_Cuenta, nuevaCuenta.Tipo_Cuenta);

                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevaCuenta.ID_Cuenta }, new CuentaResponse
                {
                    Codigo = 0,
                    Descripcion = "Cuenta creada exitosamente",
                    Cuenta = cuentaDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error general al crear cuenta");
                _logger.LogError("Mensaje: {Message}", ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError("InnerException: {InnerMessage}", ex.InnerException.Message);
                }

                return StatusCode(500, new CuentaResponse
                {
                    Codigo = -1,
                    Descripcion = $"Error interno del servidor: {ex.Message}"
                });
            }
        }

        // Método para generar número de cuenta
        private string GenerarNumeroCuenta(int clienteId, string tipoCuenta)
        {
            string codigoCliente = clienteId.ToString().PadLeft(4, '0');
            string timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            string codigoTipo = tipoCuenta.ToUpper() == "CORRIENTE" ? "C" : "A";

            string numero = $"CR{codigoCliente}{timestamp}{codigoTipo}";

            // Limitar a 22 caracteres (máximo de la columna)
            if (numero.Length > 22) numero = numero.Substring(0, 22);

            return numero;
        }

        private string GenerarNumeroCuentaAlternativo(int clienteId, string tipoCuenta)
        {
            string codigoCliente = clienteId.ToString().PadLeft(4, '0');
            string guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            string codigoTipo = tipoCuenta.ToUpper() == "CORRIENTE" ? "C" : "A";

            string numero = $"CR{codigoCliente}{guid}{codigoTipo}";

            if (numero.Length > 22) numero = numero.Substring(0, 22);

            return numero;
        }

        // PUT: api/coreaccount/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<CuentaResponse>> Actualizar(int id, [FromBody] ActualizarCuentaRequest request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

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

                var cuentaAnterior = new
                {
                    cuentaExistente.Numero_Cuenta,
                    cuentaExistente.Tipo_Cuenta,
                    cuentaExistente.Identificacion_Cliente,
                    cuentaExistente.Saldo,
                    cuentaExistente.ID_Estado
                };

                if (request.EstadoId.HasValue)
                {
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

                if (request.Saldo.HasValue)
                {
                    cuentaExistente.Saldo = request.Saldo.Value;
                }

                if (!string.IsNullOrWhiteSpace(request.TipoCuenta))
                {
                    var tiposValidos = new List<string> { "CORRIENTE", "AHORROS" };
                    if (!tiposValidos.Contains(request.TipoCuenta.ToUpper()))
                    {
                        return BadRequest(new CuentaResponse
                        {
                            Codigo = -1,
                            Descripcion = $"Tipo de cuenta inválido. Los tipos válidos son: {string.Join(", ", tiposValidos)}"
                        });
                    }
                    cuentaExistente.Tipo_Cuenta = request.TipoCuenta.ToUpper();
                }

                await _context.SaveChangesAsync();

                var cliente = await _context.ClientesBanco
                    .FirstOrDefaultAsync(c => c.ID_Cliente == cuentaExistente.Identificacion_Cliente);

                var jsonAnterior = System.Text.Json.JsonSerializer.Serialize(cuentaAnterior);
                var jsonActual = System.Text.Json.JsonSerializer.Serialize(new
                {
                    cuentaExistente.Numero_Cuenta,
                    cuentaExistente.Tipo_Cuenta,
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
                    TipoCuenta = cuentaExistente.Tipo_Cuenta ?? "AHORROS",
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

                var tieneMovimientos = await _context.MovimientosCuenta
                    .AnyAsync(m => m.Numero_Cuenta == cuentaExistente.Numero_Cuenta);

                if (tieneMovimientos)
                {
                    var cantidadMovimientos = await _context.MovimientosCuenta
                        .CountAsync(m => m.Numero_Cuenta == cuentaExistente.Numero_Cuenta);

                    return BadRequest(new CuentaResponse
                    {
                        Codigo = -1,
                        Descripcion = $"No se puede eliminar la cuenta porque tiene {cantidadMovimientos} movimiento(s) asociado(s)"
                    });
                }

                var cuentaEliminada = new
                {
                    cuentaExistente.ID_Cuenta,
                    cuentaExistente.Numero_Cuenta,
                    cuentaExistente.Identificacion_Cliente,
                    cuentaExistente.Saldo,
                    cuentaExistente.ID_Estado,
                    cuentaExistente.Tipo_Cuenta
                };

                _context.Cuentas.Remove(cuentaExistente);
                await _context.SaveChangesAsync();

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
                return StatusCode(500, new CuentaResponse
                {
                    Codigo = -1,
                    Descripcion = "Error interno del servidor"
                });
            }
        }
    }

    // DTOs actualizados con TipoCuenta
    public class CuentaDTO
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; } = "";
        public string TipoCuenta { get; set; } = "AHORROS";
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
        public int ClienteId { get; set; }
        public string TipoCuenta { get; set; } = "";
    }

    public class ActualizarCuentaRequest
    {
        public int Id { get; set; }
        public decimal? Saldo { get; set; }
        public int? EstadoId { get; set; }
        public string? TipoCuenta { get; set; }
    }
}