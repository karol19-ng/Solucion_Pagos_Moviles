using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AbstractDataAccess.Models;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging; // Agregar esto

namespace Services.Implementations
{
    public class RolService : IRolService
    {
        private readonly PagosMovilesDbContext _context;
        private readonly IBitacoraService _bitacoraService;
        private readonly ILogger<RolService> _logger; // Agregar logger

        public RolService(
            PagosMovilesDbContext context,
            IBitacoraService bitacoraService,
            ILogger<RolService> logger) // Inyectar logger
        {
            _context = context;
            _bitacoraService = bitacoraService;
            _logger = logger;
        }

        public async Task<List<RolResponse>> GetAllAsync()
        {
            var roles = await _context.Roles.ToListAsync();
            var response = new List<RolResponse>();

            foreach (var rol in roles)
            {
                var pantallas = await _context.RolPorPantallas
                    .Where(rp => rp.ID_Rol == rol.ID_Rol)
                    .Select(rp => rp.ID_Pantalla)
                    .ToListAsync();

                var pantallasDetalle = await _context.TablaPantallas
                    .Where(p => pantallas.Contains(p.ID_Pantalla))
                    .Select(p => new PantallaResponse
                    {
                        ID_Pantalla = p.ID_Pantalla,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Ruta = p.Ruta
                    })
                    .ToListAsync();

                response.Add(new RolResponse
                {
                    ID_Rol = rol.ID_Rol,
                    Nombre = rol.Nombre,
                    Descripcion = rol.Descripcion ?? "",  // ✅ Incluir Descripcion
                    Pantallas = pantallasDetalle
                });
            }

            return response;
        }

        public async Task<RolResponse> GetByIdAsync(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return null;

            var pantallas = await _context.RolPorPantallas
                .Where(rp => rp.ID_Rol == rol.ID_Rol)
                .Select(rp => rp.ID_Pantalla)
                .ToListAsync();

            var pantallasDetalle = await _context.TablaPantallas
                .Where(p => pantallas.Contains(p.ID_Pantalla))
                .Select(p => new PantallaResponse
                {
                    ID_Pantalla = p.ID_Pantalla,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Ruta = p.Ruta
                })
                .ToListAsync();

            return new RolResponse
            {
                ID_Rol = rol.ID_Rol,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion ?? "",  // ✅ Incluir Descripcion
                Pantallas = pantallasDetalle
            };
        }

        public async Task<RolResponse> CreateAsync(RolRequest request, string usuarioEjecutor)
        {
            try
            {
                _logger.LogInformation("=== CREANDO ROL EN BD (ID Manual) ===");
                _logger.LogInformation("Request: {@Request}", request);

                ValidarRol(request);

                // Obtener el máximo ID actual
                var maxId = await _context.Roles
                    .OrderByDescending(r => r.ID_Rol)
                    .Select(r => (int?)r.ID_Rol)
                    .FirstOrDefaultAsync();

                // Si no hay registros, empezar desde 1
                int nuevoId = maxId.HasValue ? maxId.Value + 1 : 1;

                _logger.LogInformation("Máximo ID actual: {MaxId}, Nuevo ID asignado: {NuevoId}", maxId, nuevoId);

                var rol = new Rol
                {
                    ID_Rol = nuevoId,  // Asignar ID manualmente
                    Nombre = request.Nombre
                };

                _context.Roles.Add(rol);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Rol guardado en BD con ID: {Id}", rol.ID_Rol);

                // Agregar pantallas
                if (request.Pantallas != null && request.Pantallas.Any())
                {
                    foreach (var pantallaId in request.Pantallas)
                    {
                        _context.RolPorPantallas.Add(new RolPorPantalla
                        {
                            ID_Rol = rol.ID_Rol,
                            ID_Pantalla = pantallaId
                        });
                    }
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("{Count} pantallas asignadas al rol", request.Pantallas.Count);
                }

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuarioEjecutor,
                    Accion = "CREAR_ROL",
                    Descripcion = System.Text.Json.JsonSerializer.Serialize(request),
                    Servicio = "/rol",
                    Resultado = "OK"
                });

                return await GetByIdAsync(rol.ID_Rol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol en BD");
                _logger.LogError("Mensaje: {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("InnerException: {InnerMessage}", ex.InnerException.Message);
                }
                throw;
            }
        }

        public async Task<RolResponse> UpdateAsync(int id, RolRequest request, string usuarioEjecutor)
        {
            try
            {
                _logger.LogInformation("=== ACTUALIZANDO ROL {Id} ===", id);
                _logger.LogInformation("Nombre: {Nombre}, Pantallas: {Pantallas}",
                    request.Nombre, string.Join(",", request.Pantallas));

                ValidarRol(request);

                var rol = await _context.Roles.FindAsync(id);
                if (rol == null)
                {
                    _logger.LogWarning("Rol {Id} no encontrado", id);
                    return null;
                }

                var anterior = System.Text.Json.JsonSerializer.Serialize(await GetByIdAsync(id));

                rol.Nombre = request.Nombre;

                // Actualizar pantallas
                var existentes = _context.RolPorPantallas.Where(rp => rp.ID_Rol == id);
                _context.RolPorPantallas.RemoveRange(existentes);

                foreach (var pantallaId in request.Pantallas)
                {
                    _context.RolPorPantallas.Add(new RolPorPantalla
                    {
                        ID_Rol = id,
                        ID_Pantalla = pantallaId
                    });
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Rol {Id} actualizado exitosamente", id);

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuarioEjecutor,
                    Accion = "MODIFICAR_ROL",
                    Descripcion = $"Anterior: {anterior} | Actual: {System.Text.Json.JsonSerializer.Serialize(request)}",
                    Servicio = "/rol",
                    Resultado = "OK"
                });

                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, string usuarioEjecutor)
        {
            try
            {
                _logger.LogInformation("=== ELIMINANDO ROL {Id} ===", id);

                var rol = await _context.Roles.FindAsync(id);
                if (rol == null)
                {
                    _logger.LogWarning("Rol {Id} no encontrado", id);
                    return false;
                }

                var eliminado = System.Text.Json.JsonSerializer.Serialize(await GetByIdAsync(id));

                // Primero eliminar las relaciones en Rol_Por_Pantalla
                var pantallasRelacionadas = _context.RolPorPantallas.Where(rp => rp.ID_Rol == id);
                _context.RolPorPantallas.RemoveRange(pantallasRelacionadas);

                // Luego eliminar el rol
                _context.Roles.Remove(rol);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Rol {Id} eliminado exitosamente", id);

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuarioEjecutor,
                    Accion = "ELIMINAR_ROL",
                    Descripcion = $"Rol eliminado ID:{id}",
                    Servicio = "/rol",
                    Resultado = "OK"
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rol {Id}", id);
                throw;
            }
        }

        private void ValidarRol(RolRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("Todos los datos son requeridos");

            var regex = new Regex(@"^[a-zA-Z0-9\s]+$");
            if (!regex.IsMatch(request.Nombre))
                throw new ArgumentException("Nombre del rol solo puede tener letras, números y espacios");
        }
    }
}