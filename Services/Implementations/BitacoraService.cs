using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractDataAccess.Models;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.Implementations
{
    public class BitacoraService : IBitacoraService
    {
        private readonly BitacoraDbContext _context;

        public BitacoraService(BitacoraDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarBitacoraAsync(BitacoraRegistroRequest request)
        {
            var bitacora = new Bitacora
            {
                Usuario = request.Usuario,
                Accion = request.Accion,
                Descripcion = request.Descripcion,
                FechaRegistro = DateTime.Now,
                Servicio = request.Servicio,
                Resultado = request.Resultado,
                Monto = request.Monto,
            };
            _context.Bitacoras.Add(bitacora);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BitacoraResponse>> ConsultarBitacorasAsync()
        {
            var bitacoras = await _context.Bitacoras
                .OrderByDescending(b => b.FechaRegistro)
                .ToListAsync();
            return bitacoras.Select(b => MapToResponse(b)).ToList();
        }

        public async Task<List<BitacoraResponse>> ConsultarPorUsuarioAsync(string usuario)
        {
            var bitacoras = await _context.Bitacoras
                .Where(b => b.Usuario == usuario)
                .OrderByDescending(b => b.FechaRegistro)
                .ToListAsync();
            return bitacoras.Select(b => MapToResponse(b)).ToList();
        }

        public async Task<List<BitacoraResponse>> ConsultarPorFechaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var bitacoras = await _context.Bitacoras
                .Where(b => b.FechaRegistro >= fechaInicio && b.FechaRegistro <= fechaFin)
                .OrderByDescending(b => b.FechaRegistro)
                .ToListAsync();
            return bitacoras.Select(b => MapToResponse(b)).ToList();
        }

        public async Task<List<Bitacoratransaccionresponse>> ConsultarTransaccionesAsync(DateTime? fecha)
        {
            var query = _context.Bitacoras.AsQueryable();

            if (fecha.HasValue)
                query = query.Where(b => b.FechaRegistro.Date == fecha.Value.Date);

            var resultado = await query
                .OrderByDescending(b => b.FechaRegistro)
                .ToListAsync();
            return resultado.Select(b => new Bitacoratransaccionresponse
            {
                Fecha = b.FechaRegistro,
                TelefonoOrigen = b.TelefonoOrigen.ToString(),
                TelefonoDestino = b.TelefonoDestino.ToString(),
                Monto = b.Monto
            }).ToList();
        }

        private BitacoraResponse MapToResponse(Bitacora b)
        {
            return new BitacoraResponse
            {
                BitacoraId = b.BitacoraId,
                Usuario = b.Usuario,
                Accion = b.Accion,
                Descripcion = b.Descripcion,
                FechaRegistro = b.FechaRegistro,
                Servicio = b.Servicio,
                Resultado = b.Resultado
            };
        }
    }
}