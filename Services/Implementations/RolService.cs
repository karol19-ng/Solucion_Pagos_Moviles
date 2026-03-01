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

namespace Services.Implementations
{
    public class RolService : IRolService
    {
        private readonly PagosMovilesDbContext _context;
        private readonly IBitacoraService _bitacoraService;

        public RolService(PagosMovilesDbContext context, IBitacoraService bitacoraService)
        {
            _context = context;
            _bitacoraService = bitacoraService;
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
                Pantallas = pantallasDetalle
            };
        }

        public async Task<RolResponse> CreateAsync(RolRequest request, string usuarioEjecutor)
        {
            ValidarRol(request);

            var rol = new Rol
            {
                ID_Rol = request.ID_Rol,
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

        public async Task<RolResponse> UpdateAsync(int id, RolRequest request, string usuarioEjecutor)
        {
            ValidarRol(request);

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return null;

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

        public async Task<bool> DeleteAsync(int id, string usuarioEjecutor)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return false;

            var eliminado = System.Text.Json.JsonSerializer.Serialize(await GetByIdAsync(id));

            var pantallas = _context.RolPorPantallas.Where(rp => rp.ID_Rol == id);
            _context.RolPorPantallas.RemoveRange(pantallas);

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

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
