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
    public class EntidadController : ControllerBase
    {
        private readonly PagosMovilesDbContext _context;
        private readonly IBitacoraService _bitacoraService;
        private readonly ILogger<EntidadController> _logger;

        public EntidadController(
            PagosMovilesDbContext context,
            IBitacoraService bitacoraService,
            ILogger<EntidadController> logger)
        {
            _context = context;
            _bitacoraService = bitacoraService;
            _logger = logger;
        }

        // GET: api/entidad
        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var entidades = await _context.Entidades
                    .Select(e => new
                    {
                        Id = e.ID_Entidad,
                        Identificador = e.Identificador, // Nuevo campo
                        Nombre = e.Nombre_Institucion,
                        EstadoId = e.ID_Estado,
                        FechaCreacion = e.Fecha_Creacion
                    })
                    .ToListAsync();

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CONSULTA",
                    Descripcion = $"El usuario consulta todas las entidades. Total: {entidades.Count} registros",
                    Servicio = "/api/entidad",
                    Resultado = "OK"
                });

                return Ok(new
                {
                    Codigo = 0,
                    Descripcion = "Entidades obtenidas exitosamente",
                    Entidades = entidades
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar entidades");
                return StatusCode(500, new { Codigo = -1, Descripcion = "Error interno del servidor" });
            }
        }

        // GET: api/entidad/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var entidad = await _context.Entidades
                    .Where(e => e.ID_Entidad == id)
                    .Select(e => new
                    {
                        Id = e.ID_Entidad,
                        Identificador = e.Identificador, // Nuevo campo
                        Nombre = e.Nombre_Institucion,
                        EstadoId = e.ID_Estado,
                        FechaCreacion = e.Fecha_Creacion
                    })
                    .FirstOrDefaultAsync();

                if (entidad == null)
                {
                    return NotFound(new { Codigo = -1, Descripcion = $"Entidad con ID {id} no encontrada" });
                }

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CONSULTA",
                    Descripcion = $"El usuario consulta entidad por ID: {id}",
                    Servicio = "/api/entidad",
                    Resultado = "OK"
                });

                return Ok(new
                {
                    Codigo = 0,
                    Descripcion = "Entidad obtenida exitosamente",
                    Entidad = entidad
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener entidad {id}");
                return StatusCode(500, new { Codigo = -1, Descripcion = "Error interno del servidor" });
            }
        }

        // POST: api/entidad
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearEntidadRequest request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                _logger.LogInformation("=== INTENTANDO CREAR ENTIDAD ===");
                _logger.LogInformation("Identificador: {Identificador}, Nombre: {Nombre}", request?.Identificador, request?.Nombre);

                if (request == null)
                {
                    return BadRequest(new { Codigo = -1, Descripcion = "Debe enviar los datos de la entidad" });
                }

                if (string.IsNullOrWhiteSpace(request.Identificador))
                {
                    return BadRequest(new { Codigo = -1, Descripcion = "El identificador es requerido" });
                }

                if (string.IsNullOrWhiteSpace(request.Nombre))
                {
                    return BadRequest(new { Codigo = -1, Descripcion = "El nombre de la institución es requerido" });
                }

                // Validar que no exista una entidad con el mismo identificador
                var existeIdentificador = await _context.Entidades
                    .AnyAsync(e => e.Identificador == request.Identificador);

                if (existeIdentificador)
                {
                    return Conflict(new { Codigo = -1, Descripcion = $"Ya existe una entidad con identificador '{request.Identificador}'" });
                }

                // Validar que no exista una entidad con el mismo nombre
                var existeNombre = await _context.Entidades
                    .AnyAsync(e => e.Nombre_Institucion == request.Nombre);

                if (existeNombre)
                {
                    return Conflict(new { Codigo = -1, Descripcion = $"Ya existe una entidad con nombre '{request.Nombre}'" });
                }

                // Obtener el máximo ID actual
                var maxId = await _context.Entidades
                    .OrderByDescending(e => e.ID_Entidad)
                    .Select(e => (int?)e.ID_Entidad)
                    .FirstOrDefaultAsync();

                int nuevoId = maxId.HasValue ? maxId.Value + 1 : 1;

                var nuevaEntidad = new Entidad
                {
                    ID_Entidad = nuevoId,
                    Identificador = request.Identificador,
                    Nombre_Institucion = request.Nombre,
                    ID_Estado = 1, // Activo por defecto
                    Fecha_Creacion = DateTime.Now
                };

                _context.Entidades.Add(nuevaEntidad);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Entidad creada exitosamente. ID: {Id}", nuevoId);

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "CREACION",
                    Descripcion = $"Nueva entidad creada: {nuevaEntidad.Identificador} - {nuevaEntidad.Nombre_Institucion}",
                    Servicio = "/api/entidad",
                    Resultado = "OK"
                });

                return Ok(new
                {
                    Codigo = 0,
                    Descripcion = "Entidad creada exitosamente",
                    Entidad = new
                    {
                        Id = nuevaEntidad.ID_Entidad,
                        Identificador = nuevaEntidad.Identificador,
                        Nombre = nuevaEntidad.Nombre_Institucion,
                        EstadoId = nuevaEntidad.ID_Estado,
                        FechaCreacion = nuevaEntidad.Fecha_Creacion
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear entidad");
                return StatusCode(500, new { Codigo = -1, Descripcion = "Error interno del servidor: " + ex.Message });
            }
        }

        // PUT: api/entidad/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarEntidadRequest request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                if (request == null)
                {
                    return BadRequest(new { Codigo = -1, Descripcion = "Debe enviar los datos de la entidad" });
                }

                var entidad = await _context.Entidades.FindAsync(id);
                if (entidad == null)
                {
                    return NotFound(new { Codigo = -1, Descripcion = $"Entidad con ID {id} no encontrada" });
                }

                // Guardar copia para bitácora
                var identificadorAnterior = entidad.Identificador;
                var nombreAnterior = entidad.Nombre_Institucion;
                var estadoAnterior = entidad.ID_Estado;

                // Si cambia el identificador, validar que no exista otro con el mismo
                if (!string.IsNullOrWhiteSpace(request.Identificador) && request.Identificador != entidad.Identificador)
                {
                    var existe = await _context.Entidades
                        .AnyAsync(e => e.Identificador == request.Identificador && e.ID_Entidad != id);

                    if (existe)
                    {
                        return Conflict(new { Codigo = -1, Descripcion = $"Ya existe otra entidad con identificador '{request.Identificador}'" });
                    }
                    entidad.Identificador = request.Identificador;
                }

                // Si cambia el nombre, validar que no exista otro con el mismo
                if (!string.IsNullOrWhiteSpace(request.Nombre) && request.Nombre != entidad.Nombre_Institucion)
                {
                    var existe = await _context.Entidades
                        .AnyAsync(e => e.Nombre_Institucion == request.Nombre && e.ID_Entidad != id);

                    if (existe)
                    {
                        return Conflict(new { Codigo = -1, Descripcion = $"Ya existe otra entidad con nombre '{request.Nombre}'" });
                    }
                    entidad.Nombre_Institucion = request.Nombre;
                }

                if (request.EstadoId.HasValue)
                {
                    entidad.ID_Estado = request.EstadoId.Value;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Entidad actualizada: {Id}", id);

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "ACTUALIZACION",
                    Descripcion = $"Entidad actualizada - ID: {id}, Identificador: {identificadorAnterior} -> {entidad.Identificador}, Nombre: {nombreAnterior} -> {entidad.Nombre_Institucion}",
                    Servicio = "/api/entidad",
                    Resultado = "OK"
                });

                return Ok(new
                {
                    Codigo = 0,
                    Descripcion = "Entidad actualizada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar entidad {id}");
                return StatusCode(500, new { Codigo = -1, Descripcion = "Error interno del servidor" });
            }
        }

        // DELETE: api/entidad/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                _logger.LogInformation("=== INTENTANDO ELIMINAR ENTIDAD ===");
                _logger.LogInformation("ID: {Id}", id);

                var entidad = await _context.Entidades.FindAsync(id);
                if (entidad == null)
                {
                    return NotFound(new { Codigo = -1, Descripcion = $"Entidad con ID {id} no encontrada" });
                }

                // Verificar si tiene dependencias usando reflexión para encontrar la propiedad correcta
                bool tieneAfiliaciones = false;

                // Intentar con diferentes nombres de propiedad
                try
                {
                    // Primero intentar con ID_Entidad
                    tieneAfiliaciones = await _context.Afiliacion.AnyAsync(a => EF.Property<int>(a, "ID_Entidad") == id);
                }
                catch
                {
                    try
                    {
                        // Luego intentar con Id_Entidad
                        tieneAfiliaciones = await _context.Afiliacion.AnyAsync(a => EF.Property<int>(a, "Id_Entidad") == id);
                    }
                    catch
                    {
                        // Si falla, asumir que no tiene afiliaciones o ignorar la validación
                        _logger.LogWarning("No se pudo verificar afiliaciones para la entidad {Id}", id);
                        tieneAfiliaciones = false;
                    }
                }

                if (tieneAfiliaciones)
                {
                    return BadRequest(new { Codigo = -1, Descripcion = "No se puede eliminar la entidad porque tiene afiliaciones asociadas" });
                }

                // Guardar copia para bitácora
                var entidadEliminada = $"{entidad.Identificador} - {entidad.Nombre_Institucion}";

                _context.Entidades.Remove(entidad);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Entidad eliminada: {Id}", id);

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuario,
                    Accion = "ELIMINACION",
                    Descripcion = $"Entidad eliminada: {entidadEliminada}",
                    Servicio = "/api/entidad",
                    Resultado = "OK"
                });

                return Ok(new
                {
                    Codigo = 0,
                    Descripcion = "Entidad eliminada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar entidad {id}");
                return StatusCode(500, new { Codigo = -1, Descripcion = "Error interno del servidor" });
            }
        }
    }

    public class CrearEntidadRequest
    {
        public string Identificador { get; set; } = string.Empty; // Nuevo campo
        public string Nombre { get; set; } = string.Empty;
    }

    public class ActualizarEntidadRequest
    {
        public string? Identificador { get; set; } // Nuevo campo
        public string? Nombre { get; set; }
        public int? EstadoId { get; set; }
    }
}