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
    public class PantallaService : IPantallaService
    {
        private readonly PagosMovilesDbContext _context;
        private readonly IBitacoraService _bitacoraService;

        public PantallaService(PagosMovilesDbContext context, IBitacoraService bitacoraService)
        {
            _context = context;
            _bitacoraService = bitacoraService;
        }

        public async Task<List<PantallaResponse>> GetAllAsync()
        {
            var pantallas = await _context.TablaPantallas.ToListAsync();
            return pantallas.Select(p => MapToResponse(p)).ToList();
        }

        public async Task<PantallaResponse> GetByIdAsync(int id)
        {
            var pantalla = await _context.TablaPantallas.FindAsync(id);
            return pantalla == null ? null : MapToResponse(pantalla);
        }

        public async Task<PantallaResponse> CreateAsync(PantallaRequest request, string usuarioEjecutor)
        {
            ValidarPantalla(request);

            var pantalla = new TablaPantalla
            {
                ID_Pantalla = request.ID_Pantalla,
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Ruta = request.Ruta,
                Estado = 1
            };

            _context.TablaPantallas.Add(pantalla);
            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "CREAR_PANTALLA",
                Descripcion = System.Text.Json.JsonSerializer.Serialize(request),
                Servicio = "/screen",
                Resultado = "OK"
            });

            return MapToResponse(pantalla);
        }

        public async Task<PantallaResponse> UpdateAsync(int id, PantallaRequest request, string usuarioEjecutor)
        {
            ValidarPantalla(request);

            var pantalla = await _context.TablaPantallas.FindAsync(id);
            if (pantalla == null) return null;

            var anterior = System.Text.Json.JsonSerializer.Serialize(MapToResponse(pantalla));

            pantalla.Nombre = request.Nombre;
            pantalla.Descripcion = request.Descripcion;
            pantalla.Ruta = request.Ruta;

            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "MODIFICAR_PANTALLA",
                Descripcion = $"Anterior: {anterior} | Actual: {System.Text.Json.JsonSerializer.Serialize(request)}",
                Servicio = "/screen",
                Resultado = "OK"
            });

            return MapToResponse(pantalla);
        }

        public async Task<bool> DeleteAsync(int id, string usuarioEjecutor)
        {
            var pantalla = await _context.TablaPantallas.FindAsync(id);
            if (pantalla == null) return false;

            var eliminado = System.Text.Json.JsonSerializer.Serialize(MapToResponse(pantalla));

            _context.TablaPantallas.Remove(pantalla);
            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "ELIMINAR_PANTALLA",
                Descripcion = $"Eliminado: {eliminado}",
                Servicio = "/screen",
                Resultado = "OK"
            });

            return true;
        }

        private void ValidarPantalla(PantallaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Descripcion) ||
                string.IsNullOrWhiteSpace(request.Ruta))
                throw new ArgumentException("Todos los datos son requeridos");

            var regex = new Regex(@"^[a-zA-Z0-9\s]+$");
            if (!regex.IsMatch(request.Nombre) || !regex.IsMatch(request.Descripcion))
                throw new ArgumentException("Nombre y descripción solo pueden tener letras, números y espacios");
        }

        private PantallaResponse MapToResponse(TablaPantalla p)
        {
            return new PantallaResponse
            {
                ID_Pantalla = p.ID_Pantalla,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Ruta = p.Ruta
            };
        }
    }
}