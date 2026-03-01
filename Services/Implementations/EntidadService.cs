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
    public class EntidadService : IEntidadService
    {
        private readonly PagosMovilesDbContext _context;
        private readonly IBitacoraService _bitacoraService;

        public EntidadService(PagosMovilesDbContext context, IBitacoraService bitacoraService)
        {
            _context = context;
            _bitacoraService = bitacoraService;
        }

        public async Task<List<EntidadResponse>> GetAllAsync()
        {
            var entidades = await _context.Entidades.ToListAsync();
            return entidades.Select(e => MapToResponse(e)).ToList();
        }

        public async Task<EntidadResponse> GetByIdAsync(int id)
        {
            var entidad = await _context.Entidades.FindAsync(id);
            return entidad == null ? null : MapToResponse(entidad);
        }

        public async Task<EntidadResponse> CreateAsync(EntidadRequest request, string usuarioEjecutor)
        {
            ValidarEntidad(request);

            var entidad = new Entidad
            {
                ID_Entidad = request.ID_Entidad,
                Nombre_Institucion = request.Nombre_Institucion,
                ID_Estado = 1,
                Fecha_Creacion = DateTime.Now
            };

            _context.Entidades.Add(entidad);
            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "CREAR_ENTIDAD",
                Descripcion = System.Text.Json.JsonSerializer.Serialize(request),
                Servicio = "/entidad",
                Resultado = "OK"
            });

            return MapToResponse(entidad);
        }

        public async Task<EntidadResponse> UpdateAsync(int id, EntidadRequest request, string usuarioEjecutor)
        {
            ValidarEntidad(request);

            var entidad = await _context.Entidades.FindAsync(id);
            if (entidad == null) return null;

            var anterior = System.Text.Json.JsonSerializer.Serialize(MapToResponse(entidad));

            entidad.Nombre_Institucion = request.Nombre_Institucion;

            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "MODIFICAR_ENTIDAD",
                Descripcion = $"Anterior: {anterior} | Actual: {System.Text.Json.JsonSerializer.Serialize(request)}",
                Servicio = "/entidad",
                Resultado = "OK"
            });

            return MapToResponse(entidad);
        }

        public async Task<bool> DeleteAsync(int id, string usuarioEjecutor)
        {
            var entidad = await _context.Entidades.FindAsync(id);
            if (entidad == null) return false;

            var eliminado = System.Text.Json.JsonSerializer.Serialize(MapToResponse(entidad));

            _context.Entidades.Remove(entidad);
            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "ELIMINAR_ENTIDAD",
                Descripcion = $"Eliminado: {eliminado}",
                Servicio = "/entidad",
                Resultado = "OK"
            });

            return true;
        }

        private void ValidarEntidad(EntidadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre_Institucion))
                throw new ArgumentException("Todos los datos son requeridos");

            if (!Regex.IsMatch(request.Nombre_Institucion, @"^[a-zA-Z\s]+$"))
                throw new ArgumentException("Nombre de institución solo permite letras y espacios");
        }

        private EntidadResponse MapToResponse(Entidad e)
        {
            return new EntidadResponse
            {
                ID_Entidad = e.ID_Entidad,
                Nombre_Institucion = e.Nombre_Institucion
            };
        }
    }
}