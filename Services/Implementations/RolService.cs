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
            try
            {
                _logger.LogInformation("=== OBTENIENDO TODOS LOS ROLES ===");

                var roles = await _context.Roles.ToListAsync();
                _logger.LogInformation("Roles encontrados en BD: {Count}", roles.Count);

                var response = new List<RolResponse>();

                foreach (var rol in roles)
                {
                    _logger.LogDebug("Procesando rol ID: {Id}, Nombre: {Nombre}", rol.ID_Rol, rol.Nombre);

                    var pantallas = await _context.RolPorPantallas
                        .Where(rp => rp.ID_Rol == rol.ID_Rol)
                        .Select(rp => rp.ID_Pantalla)
                        .ToListAsync();

                    _logger.LogDebug("Rol {Id} tiene {Count} pantallas asignadas", rol.ID_Rol, pantallas.Count);

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
                        Pantallas = pantallasDetalle
                    });
                }

                _logger.LogInformation("GetAllAsync completado. Total roles: {Count}", response.Count);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAllAsync: {Message}", ex.Message);
                throw; // Re-lanzar la excepción para que el controlador la maneje
            }
        }

        public async Task<RolResponse> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("=== OBTENIENDO ROL POR ID {Id} ===", id);

                var rol = await _context.Roles.FindAsync(id);
                if (rol == null)
                {
                    _logger.LogWarning("Rol {Id} no encontrado", id);
                    return null;
                }

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
                    Pantallas = pantallasDetalle
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetByIdAsync para ID {Id}", id);
                throw;
            }
        }

        public async Task<RolResponse> CreateAsync(RolRequest request, string usuarioEjecutor)
        {
            try
            {
                _logger.LogInformation("=== CREANDO NUEVO ROL ===");
                _logger.LogInformation("Nombre: {Nombre}, Pantallas: {Pantallas}",
                    request.Nombre, string.Join(",", request.Pantallas));

                ValidarRol(request);

                // Obtener el máximo ID actual
                var maxId = await _context.Roles
                    .OrderByDescending(r => r.ID_Rol)
                    .Select(r => (int?)r.ID_Rol)
                    .FirstOrDefaultAsync();

                int nuevoId = maxId.HasValue ? maxId.Value + 1 : 1;
                _logger.LogInformation("ID asignado: {Id}", nuevoId);

                var rol = new Rol
                {
                    ID_Rol = nuevoId,
                    Nombre = request.Nombre
                };

                _context.Roles.Add(rol);
                await _context.SaveChangesAsync();

                // Agregar pantallas
                foreach (var pantallaId in request.Pantallas)
                {
                    _context.RolPorPantallas.Add(new RolPorPantalla
                    {
                        ID_Rol = rol.ID_Rol,
                        ID_Pantalla = pantallaId
                    });
                }
                await _context.SaveChangesAsync();

                _logger.LogInformation("Rol {Id} creado exitosamente", rol.ID_Rol);

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
                _logger.LogError(ex, "Error al crear rol");
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

                // Eliminar primero las relaciones
                var pantallas = _context.RolPorPantallas.Where(rp => rp.ID_Rol == id);
                _context.RolPorPantallas.RemoveRange(pantallas);

                // Luego eliminar el rol
                _context.Roles.Remove(rol);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Rol {Id} eliminado exitosamente", id);

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuarioEjecutor,
                    Accion = "ELIMINAR_ROL",
                    Descripcion = $"Eliminado: {eliminado}",
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