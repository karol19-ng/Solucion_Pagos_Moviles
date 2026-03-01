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
    public class UsuarioService : IUsuarioService
    {
        private readonly PagosMovilesDbContext _context;
        private readonly IBitacoraService _bitacoraService;

        public UsuarioService(PagosMovilesDbContext context, IBitacoraService bitacoraService)
        {
            _context = context;
            _bitacoraService = bitacoraService;
        }

        public async Task<List<UsuarioResponse>> GetAllAsync()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return usuarios.Select(u => MapToResponse(u)).ToList();
        }

        public async Task<UsuarioResponse> GetByIdAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            return usuario == null ? null : MapToResponse(usuario);
        }

        public async Task<List<UsuarioResponse>> GetByFilterAsync(string identificacion, string nombre, string tipo)
        {
            var query = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrEmpty(identificacion))
                query = query.Where(u => u.Identificacion.Contains(identificacion));

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(u => u.Nombre_Completo.Contains(nombre));

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(u => u.Tipo_Identificacion == tipo);

            var usuarios = await query.ToListAsync();
            return usuarios.Select(u => MapToResponse(u)).ToList();
        }

        public async Task<UsuarioResponse> CreateAsync(UsuarioRequest request, string usuarioEjecutor)
        {
            // Validaciones SRV1
            if (string.IsNullOrWhiteSpace(request.Nombre_Completo))
                throw new ArgumentException("El nombre completo no puede ser vacío ni espacios en blanco");

            if (!IsValidEmail(request.Email))
                throw new ArgumentException("Formato de email inválido");

            var usuario = new Usuario
            {
                ID_Usuario = request.ID_Usuario,
                Nombre_Completo = request.Nombre_Completo,
                Tipo_Identificacion = request.Tipo_Identificacion,
                Identificacion = request.Identificacion,
                Email = request.Email,
                Telefono = request.Telefono,
                NombreUsuario = request.Usuario,
                Contraseña = BCrypt.Net.BCrypt.HashPassword(request.Contraseña),
                ID_Estado = request.ID_Estado,
                ID_Rol = request.ID_Rol,
                Fecha_Creacion = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Registrar bitácora
            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "CREAR_USUARIO",
                Descripcion = $"Nuevo registro: {System.Text.Json.JsonSerializer.Serialize(request)}",
                Servicio = "/user",
                Resultado = "OK"
            });

            return MapToResponse(usuario);
        }

        public async Task<UsuarioResponse> UpdateAsync(int id, UsuarioRequest request, string usuarioEjecutor)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return null;

            var usuarioAnterior = System.Text.Json.JsonSerializer.Serialize(MapToResponse(usuario));

            usuario.Nombre_Completo = request.Nombre_Completo;
            usuario.Tipo_Identificacion = request.Tipo_Identificacion;
            usuario.Identificacion = request.Identificacion;
            usuario.Email = request.Email;
            usuario.Telefono = request.Telefono;
            usuario.NombreUsuario = request.Usuario;
            usuario.ID_Estado = request.ID_Estado;
            usuario.ID_Rol = request.ID_Rol;

            if (!string.IsNullOrEmpty(request.Contraseña))
                usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(request.Contraseña);

            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "MODIFICAR_USUARIO",
                Descripcion = $"Anterior: {usuarioAnterior} | Actual: {System.Text.Json.JsonSerializer.Serialize(request)}",
                Servicio = "/user",
                Resultado = "OK"
            });

            return MapToResponse(usuario);
        }

        public async Task<bool> DeleteAsync(int id, string usuarioEjecutor)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;

            var usuarioEliminado = System.Text.Json.JsonSerializer.Serialize(MapToResponse(usuario));

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "ELIMINAR_USUARIO",
                Descripcion = $"Eliminado: {usuarioEliminado}",
                Servicio = "/user",
                Resultado = "OK"
            });

            return true;
        }

        private UsuarioResponse MapToResponse(Usuario u)
        {
            return new UsuarioResponse
            {
                ID_Usuario = u.ID_Usuario,
                Nombre_Completo = u.Nombre_Completo,
                Tipo_Identificacion = u.Tipo_Identificacion,
                Identificacion = u.Identificacion,
                Email = u.Email,
                Telefono = u.Telefono,
                Usuario = u.NombreUsuario,
                ID_Estado = u.ID_Estado,
                ID_Rol = u.ID_Rol
            };
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}