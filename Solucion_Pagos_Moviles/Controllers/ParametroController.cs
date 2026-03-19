using AbstractDataAccess.Models;
using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Solucion_Pagos_Moviles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ParametroController : ControllerBase
    {
        private readonly PagosMovilesDbContext _context;
        private readonly IBitacoraService _bitacoraService;
        private readonly ILogger<ParametroController> _logger;

        public ParametroController(
            PagosMovilesDbContext context,
            IBitacoraService bitacoraService,
            ILogger<ParametroController> logger)
        {
            _context = context;
            _bitacoraService = bitacoraService;
            _logger = logger;
        }

        // GET: api/parametro
        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var parametros = await _context.Parametros
                    .Select(p => new
                    {
                        Id = p.ID_Parametro,
                        Valor = p.Valor,
                        EstadoId = p.ID_Estado,
                        FechaCreacion = p.Fecha_Creacion
                    })
                    .ToListAsync();

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CONSULTA",
                    Descripcion = $"El usuario consulta todos los parámetros. Total: {parametros.Count} registros",
                    Servicio = "/api/parametro",
                    Resultado = "OK"
                });

                return Ok(new
                {
                    Codigo = 0,
                    Descripcion = "Parámetros obtenidos exitosamente",
                    Parametros = parametros
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar parámetros");
                return StatusCode(500, new { Codigo = -1, Descripcion = "Error interno del servidor" });
            }
        }

        // GET: api/parametro/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(string id)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var parametro = await _context.Parametros
                    .Where(p => p.ID_Parametro == id)
                    .Select(p => new
                    {
                        Id = p.ID_Parametro,
                        Valor = p.Valor,
                        EstadoId = p.ID_Estado,
                        FechaCreacion = p.Fecha_Creacion
                    })
                    .FirstOrDefaultAsync();

                if (parametro == null)
                {
                    return NotFound(new { Codigo = -1, Descripcion = $"Parámetro '{id}' no encontrado" });
                }

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CONSULTA",
                    Descripcion = $"El usuario consulta parámetro por ID: {id}",
                    Servicio = "/api/parametro",
                    Resultado = "OK"
                });

                return Ok(new
                {
                    Codigo = 0,
                    Descripcion = "Parámetro obtenido exitosamente",
                    Parametro = parametro
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener parámetro {id}");
                return StatusCode(500, new { Codigo = -1, Descripcion = "Error interno del servidor" });
            }
        }

        // POST: api/parametro
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearParametroRequest request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                _logger.LogInformation("=== INTENTANDO CREAR PARÁMETRO ===");
                _logger.LogInformation("ID: {Id}, Valor: {Valor}, Estado: {Estado}",
                    request?.IdParametro, request?.Valor, request?.IdEstado);

                if (request == null)
                {
                    return BadRequest(new { Codigo = -1, Descripcion = "Debe enviar los datos del parámetro" });
                }

                if (string.IsNullOrWhiteSpace(request.IdParametro))
                {
                    return BadRequest(new { Codigo = -1, Descripcion = "El ID del parámetro es requerido" });
                }

                if (string.IsNullOrWhiteSpace(request.Valor))
                {
                    return BadRequest(new { Codigo = -1, Descripcion = "El valor es requerido" });
                }

                // Validar formato del ID (solo mayúsculas, números y guiones bajos)
                if (!System.Text.RegularExpressions.Regex.IsMatch(request.IdParametro, @"^[A-Z0-9_]+$"))
                {
                    return BadRequest(new { Codigo = -1, Descripcion = "El ID solo puede contener mayúsculas, números y guiones bajos" });
                }

                // Validar que no exista
                var existe = await _context.Parametros.AnyAsync(p => p.ID_Parametro == request.IdParametro);
                if (existe)
                {
                    return Conflict(new { Codigo = -1, Descripcion = $"Ya existe un parámetro con ID '{request.IdParametro}'" });
                }

                var nuevoParametro = new Parametro
                {
                    ID_Parametro = request.IdParametro.ToUpper(),
                    Valor = request.Valor,
                    ID_Estado = request.IdEstado,
                    Fecha_Creacion = DateTime.Now
                };

                _context.Parametros.Add(nuevoParametro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Parámetro creado exitosamente: {Id}", nuevoParametro.ID_Parametro);

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CREACION",
                    Descripcion = $"Nuevo parámetro creado: {nuevoParametro.ID_Parametro} = {nuevoParametro.Valor}",
                    Servicio = "/api/parametro",
                    Resultado = "OK"
                });

                return Ok(new
                {
                    Codigo = 0,
                    Descripcion = "Parámetro creado exitosamente",
                    Parametro = new
                    {
                        Id = nuevoParametro.ID_Parametro,
                        nuevoParametro.Valor,
                        EstadoId = nuevoParametro.ID_Estado,
                        nuevoParametro.Fecha_Creacion
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear parámetro");
                return StatusCode(500, new { Codigo = -1, Descripcion = "Error interno del servidor: " + ex.Message });
            }
        }

        // PUT: api/parametro/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(string id, [FromBody] ActualizarParametroRequest request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                if (request == null)
                {
                    return BadRequest(new { Codigo = -1, Descripcion = "Debe enviar los datos del parámetro" });
                }

                var parametro = await _context.Parametros.FindAsync(id);
                if (parametro == null)
                {
                    return NotFound(new { Codigo = -1, Descripcion = $"Parámetro '{id}' no encontrado" });
                }

                // Guardar copia para bitácora
                var valorAnterior = parametro.Valor;
                var estadoAnterior = parametro.ID_Estado;

                // Actualizar campos
                if (!string.IsNullOrWhiteSpace(request.Valor))
                {
                    parametro.Valor = request.Valor;
                }

                if (request.IdEstado.HasValue)
                {
                    parametro.ID_Estado = request.IdEstado.Value;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Parámetro actualizado: {Id}", id);

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "ACTUALIZACION",
                    Descripcion = $"Parámetro actualizado: {id} - Valor anterior: {valorAnterior}, Nuevo valor: {parametro.Valor}",
                    Servicio = "/api/parametro",
                    Resultado = "OK"
                });

                return Ok(new
                {
                    Codigo = 0,
                    Descripcion = "Parámetro actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar parámetro {id}");
                return StatusCode(500, new { Codigo = -1, Descripcion = "Error interno del servidor" });
            }
        }

        // DELETE: api/parametro/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(string id)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                _logger.LogInformation("=== INTENTANDO ELIMINAR PARÁMETRO ===");
                _logger.LogInformation("ID: {Id}", id);

                var parametro = await _context.Parametros.FindAsync(id);
                if (parametro == null)
                {
                    return NotFound(new { Codigo = -1, Descripcion = $"Parámetro '{id}' no encontrado" });
                }

                // Guardar copia para bitácora
                var parametroEliminado = $"{parametro.ID_Parametro} = {parametro.Valor}";

                _context.Parametros.Remove(parametro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Parámetro eliminado: {Id}", id);

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "ELIMINACION",
                    Descripcion = $"Parámetro eliminado: {parametroEliminado}",
                    Servicio = "/api/parametro",
                    Resultado = "OK"
                });

                return Ok(new
                {
                    Codigo = 0,
                    Descripcion = "Parámetro eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar parámetro {id}");
                return StatusCode(500, new { Codigo = -1, Descripcion = "Error interno del servidor" });
            }
        }
    }

    public class CrearParametroRequest
    {
        public string IdParametro { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public int IdEstado { get; set; } = 1;
    }

    public class ActualizarParametroRequest
    {
        public string? Valor { get; set; }
        public int? IdEstado { get; set; }
    }
}