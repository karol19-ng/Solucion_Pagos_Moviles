using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AbstractDataAccess.Models;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.Implementations
{
    public class AfiliacionService : IAfiliacionService
    {
        private readonly PagosMovilesDbContext _context;
        private readonly ICoreBancarioService _coreService;
        private readonly IBitacoraService _bitacoraService;

        public AfiliacionService(
            PagosMovilesDbContext context,
            ICoreBancarioService coreService,
            IBitacoraService bitacoraService)
        {
            _context = context;
            _coreService = coreService;
            _bitacoraService = bitacoraService;
        }

        // SRV9 - Inscripción
        public async Task<AfiliacionResponse> InscribirAsync(AfiliacionRequest request, string usuarioEjecutor)
        {
            // Validar datos
            if (string.IsNullOrWhiteSpace(request.Numero_Cuenta) ||
                string.IsNullOrWhiteSpace(request.Identificacion) ||
                string.IsNullOrWhiteSpace(request.Numero_Telefono))
            {
                return new AfiliacionResponse
                {
                    Codigo = -1,
                    Descripcion = "Datos incorrectos"
                };
            }

            // Verificar cliente en core
            var clienteExiste = await _coreService.ClienteExisteAsync(request.Identificacion);
            if (!clienteExiste)
            {
                return new AfiliacionResponse
                {
                    Codigo = -1,
                    Descripcion = "Datos incorrectos"
                };
            }

            // Verificar si teléfono ya está afiliado
            var existente = await _context.Afiliacion
                .FirstOrDefaultAsync(a => a.Telefono == request.Numero_Telefono);

            if (existente != null && existente.ID_Estado == 4) // Habilitado
            {
                return new AfiliacionResponse
                {
                    Codigo = -1,
                    Descripcion = "Teléfono ya se encuentra afiliado, realice el proceso de desinscripción"
                };
            }

            if (existente != null && (existente.ID_Estado == 5 || existente.ID_Estado == 2)) // Deshabilitado o inactivo
            {
                // Reactivar
                existente.ID_Estado = 4; // Habilitado
                existente.Fecha_Actualizacion = DateTime.Now;
                await _context.SaveChangesAsync();

                await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
                {
                    Usuario = usuarioEjecutor,
                    Accion = "REACTIVAR_AFILIACION",
                    Descripcion = System.Text.Json.JsonSerializer.Serialize(request),
                    Servicio = "/auth/register",
                    Resultado = "OK"
                });

                return new AfiliacionResponse
                {
                    Codigo = 0,
                    Descripcion = "Inscripción realizada"
                };
            }

            // Nueva afiliación
            var afiliacion = new Afiliacion
            {
                Numero_Cuenta = request.Numero_Cuenta,
                Identificacion_Usuario = request.Identificacion,
                Telefono = request.Numero_Telefono,
                ID_Estado = 4, // Habilitado
                Fecha_Afiliacion = DateTime.Now
            };

            _context.Afiliacion.Add(afiliacion);
            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "CREAR_AFILIACION",
                Descripcion = System.Text.Json.JsonSerializer.Serialize(request),
                Servicio = "/auth/register",
                Resultado = "OK"
            });

            return new AfiliacionResponse
            {
                Codigo = 0,
                Descripcion = "Inscripción realizada"
            };
        }

        // SRV10 - Desinscripción
        public async Task<AfiliacionResponse> DesinscribirAsync(AfiliacionRequest request, string usuarioEjecutor)
        {
            // Validar datos
            if (string.IsNullOrWhiteSpace(request.Numero_Cuenta) ||
                string.IsNullOrWhiteSpace(request.Identificacion) ||
                string.IsNullOrWhiteSpace(request.Numero_Telefono))
            {
                return new AfiliacionResponse
                {
                    Codigo = -1,
                    Descripcion = "Datos incorrectos"
                };
            }

            var afiliacion = await _context.Afiliacion
                .FirstOrDefaultAsync(a => a.Telefono == request.Numero_Telefono);

            if (afiliacion == null || afiliacion.ID_Estado != 4)
            {
                return new AfiliacionResponse
                {
                    Codigo = -1,
                    Descripcion = "Teléfono no se encuentra afiliado"
                };
            }

            // Deshabilitar
            afiliacion.ID_Estado = 5; // Deshabilitado
            afiliacion.Fecha_Actualizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "DESINSCRIBIR_AFILIACION",
                Descripcion = System.Text.Json.JsonSerializer.Serialize(request),
                Servicio = "/auth/cancel-subscription",
                Resultado = "OK"
            });

            return new AfiliacionResponse
            {
                Codigo = 0,
                Descripcion = "Desinscripción realizada"
            };
        }

        // SRV13 - Consultar saldo
        public async Task<ConsultaSaldoResponse> ConsultarSaldoAsync(ConsultaSaldoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Telefono) || string.IsNullOrWhiteSpace(request.Identificacion))
                throw new ArgumentException("Debe enviar los datos completos y válidos");

            var afiliacion = await _context.Afiliacion
                .FirstOrDefaultAsync(a => a.Telefono == request.Telefono && a.ID_Estado == 4);

            if (afiliacion == null)
                throw new ArgumentException("Cliente no asociado a pagos móviles");

            var coreRequest = new CoreConsultaSaldoRequest
            {
                Identificacion = afiliacion.Identificacion_Usuario,
                Cuenta = afiliacion.Numero_Cuenta
            };

            var result = await _coreService.ConsultarSaldoAsync(coreRequest);

            return new ConsultaSaldoResponse { Saldo = result.saldo ?? 0 };
        }

        // SRV11 - Consultar movimientos
        public async Task<List<MovimientoDTO>> ConsultarMovimientosAsync(UltimosMovimientosRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Telefono) || string.IsNullOrWhiteSpace(request.Identificacion))
                throw new ArgumentException("Todos los datos son requeridos");

            var afiliacion = await _context.Afiliacion
                .FirstOrDefaultAsync(a => a.Telefono == request.Telefono && a.ID_Estado == 4);

            if (afiliacion == null)
                throw new ArgumentException("Cliente no asociado a pagos móviles");

            var coreRequest = new CoreConsultaMovimientosRequest
            {
                Identificacion = afiliacion.Identificacion_Usuario,
                Cuenta = afiliacion.Numero_Cuenta
            };

            return await _coreService.ConsultarMovimientosAsync(coreRequest);
        }
    }
}