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
using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class PantallaService : IPantallaService
    {
        private readonly PagosMovilesDbContext _context;
        private readonly IBitacoraService _bitacoraService;
        private readonly ILogger<PantallaService> _logger;  // Agregar logger

        public PantallaService(
        PagosMovilesDbContext context,
        IBitacoraService bitacoraService,
        ILogger<PantallaService> logger)
        {
            _context = context;
            _bitacoraService = bitacoraService;
            _logger = logger;  // ✅ Agregar esta línea
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
            _logger.LogInformation("=== CREANDO PANTALLA EN BD (ID Manual) ===");
            _logger.LogInformation("Request: {@Request}", request);
            _logger.LogInformation("Usuario: {Usuario}", usuarioEjecutor);

            ValidarPantalla(request);

            // Obtener el máximo ID actual
            var maxId = await _context.TablaPantallas
                .OrderByDescending(p => p.ID_Pantalla)
                .Select(p => (int?)p.ID_Pantalla)
                .FirstOrDefaultAsync();

            // Si no hay registros, empezar desde 1
            int nuevoId = maxId.HasValue ? maxId.Value + 1 : 1;

            _logger.LogInformation("Máximo ID actual: {MaxId}, Nuevo ID asignado: {NuevoId}", maxId, nuevoId);

            var pantalla = new TablaPantalla
            {
                ID_Pantalla = nuevoId,  // Asignar ID manualmente
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Ruta = request.Ruta,
                Estado = 1
            };

            _context.TablaPantallas.Add(pantalla);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Pantalla creada con ID: {Id}", pantalla.ID_Pantalla);

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
            _logger.LogInformation("=== ELIMINANDO PANTALLA EN BD ===");
            _logger.LogInformation("ID recibido: {Id}", id);

            // Buscar por ID exacto
            var pantalla = await _context.TablaPantallas.FindAsync(id);

            if (pantalla == null)
            {
                _logger.LogWarning("No se encontró pantalla con ID {Id}", id);
                return false;
            }

            _logger.LogInformation("Eliminando pantalla: {Nombre} (ID: {Id})", pantalla.Nombre, pantalla.ID_Pantalla);

            try
            {
                _context.TablaPantallas.Remove(pantalla);
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation("Resultado de eliminación: {Result}", result > 0 ? "Éxito" : "Falló");

                if (result > 0)
                {
                    await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                    {
                        Usuario = usuarioEjecutor,
                        Accion = "ELIMINAR_PANTALLA",
                        Descripcion = $"Pantalla eliminada: {pantalla.Nombre} (ID:{id})",
                        Servicio = "/screen",
                        Resultado = "OK"
                    });

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pantalla {Id}", id);
                return false;
            }
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