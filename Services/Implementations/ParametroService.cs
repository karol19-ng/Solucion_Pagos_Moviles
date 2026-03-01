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
    public class ParametroService : IParametroService
    {
        private readonly PagosMovilesDbContext _context;
        private readonly IBitacoraService _bitacoraService;

        public ParametroService(PagosMovilesDbContext context, IBitacoraService bitacoraService)
        {
            _context = context;
            _bitacoraService = bitacoraService;
        }

        public async Task<List<ParametroResponse>> GetAllAsync()
        {
            var parametros = await _context.Parametros.ToListAsync();
            return parametros.Select(p => MapToResponse(p)).ToList();
        }

        public async Task<ParametroResponse> GetByIdAsync(string id)
        {
            var parametro = await _context.Parametros.FindAsync(id);
            return parametro == null ? null : MapToResponse(parametro);
        }

        public async Task<ParametroResponse> CreateAsync(ParametroRequest request, string usuarioEjecutor)
        {
            ValidarParametro(request);

            var parametro = new Parametro
            {
                ID_Parametro = request.ID_Parametro,
                Valor = request.Valor,
                ID_Estado = 1,
                Fecha_Creacion = DateTime.Now
            };

            _context.Parametros.Add(parametro);
            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "CREAR_PARAMETRO",
                Descripcion = System.Text.Json.JsonSerializer.Serialize(request),
                Servicio = "/parametro",
                Resultado = "OK"
            });

            return MapToResponse(parametro);
        }

        public async Task<ParametroResponse> UpdateAsync(string id, ParametroRequest request, string usuarioEjecutor)
        {
            ValidarParametro(request);

            var parametro = await _context.Parametros.FindAsync(id);
            if (parametro == null) return null;

            var anterior = System.Text.Json.JsonSerializer.Serialize(MapToResponse(parametro));

            parametro.Valor = request.Valor;

            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "MODIFICAR_PARAMETRO",
                Descripcion = $"Anterior: {anterior} | Actual: {System.Text.Json.JsonSerializer.Serialize(request)}",
                Servicio = "/parametro",
                Resultado = "OK"
            });

            return MapToResponse(parametro);
        }

        public async Task<bool> DeleteAsync(string id, string usuarioEjecutor)
        {
            var parametro = await _context.Parametros.FindAsync(id);
            if (parametro == null) return false;

            var eliminado = System.Text.Json.JsonSerializer.Serialize(MapToResponse(parametro));

            _context.Parametros.Remove(parametro);
            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "ELIMINAR_PARAMETRO",
                Descripcion = $"Eliminado: {eliminado}",
                Servicio = "/parametro",
                Resultado = "OK"
            });

            return true;
        }

        private void ValidarParametro(ParametroRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ID_Parametro) || string.IsNullOrWhiteSpace(request.Valor))
                throw new ArgumentException("Todos los datos son requeridos");

            if (request.ID_Parametro.Length > 10)
                throw new ArgumentException("Identificador máximo 10 caracteres");

            if (!Regex.IsMatch(request.ID_Parametro, @"^[A-Z]+$"))
                throw new ArgumentException("Identificador solo permite letras mayúsculas");

            if (request.Valor.Length > 500)
                throw new ArgumentException("Valor máximo 500 caracteres");
        }

        private ParametroResponse MapToResponse(Parametro p)
        {
            return new ParametroResponse
            {
                ID_Parametro = p.ID_Parametro,
                Valor = p.Valor
            };
        }
    }
}