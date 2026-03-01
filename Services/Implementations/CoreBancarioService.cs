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
    public class CoreBancarioService : ICoreBancarioService
    {
        private readonly CoreBancarioDbContext _context;
        private readonly IBitacoraService _bitacoraService;

        public CoreBancarioService(CoreBancarioDbContext context, IBitacoraService bitacoraService)
        {
            _context = context;
            _bitacoraService = bitacoraService;
        }

        // SRV14 - Aplicar transacción en core
        public async Task<CoreOperacionResponse> AplicarTransaccionAsync(CoreTransaccionRequest request, string usuarioEjecutor)
        {
            // Buscar cliente
            var cliente = await _context.ClientesBanco
                .FirstOrDefaultAsync(c => c.Identificacion == request.Identificacion);

            if (cliente == null)
            {
                return new CoreOperacionResponse
                {
                    codigo = -1,
                    descripcion = "Cliente no encontrado"
                };
            }

            // Buscar cuenta
            var cuenta = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Identificacion_Cliente == cliente.ID_Cliente);

            if (cuenta == null)
            {
                return new CoreOperacionResponse
                {
                    codigo = -1,
                    descripcion = "Cuenta no encontrada"
                };
            }

            decimal saldoAnterior = cuenta.Saldo;
            decimal saldoNuevo = saldoAnterior;

            if (request.Tipo_Movimiento.ToUpper() == "DEBITO")
            {
                if (saldoAnterior < request.Monto)
                {
                    return new CoreOperacionResponse
                    {
                        codigo = -1,
                        descripcion = "Saldo insuficiente"
                    };
                }
                saldoNuevo -= request.Monto;
            }
            else if (request.Tipo_Movimiento.ToUpper() == "CREDITO")
            {
                saldoNuevo += request.Monto;
            }
            else
            {
                return new CoreOperacionResponse
                {
                    codigo = -1,
                    descripcion = "Tipo de movimiento inválido"
                };
            }

            // Actualizar saldo
            cuenta.Saldo = saldoNuevo;

            // Registrar movimiento
            var movimiento = new MovimientoCuenta
            {
                Numero_Cuenta = cuenta.Numero_Cuenta,
                FechaMovimiento = DateTime.Now,
                Monto = request.Monto,
                TipoMovimiento = request.Tipo_Movimiento.ToUpper(),
                Descripcion = "Transacción pagos móviles",
                SaldoAnterior = saldoAnterior,
                SaldoNuevo = saldoNuevo
            };

            _context.MovimientosCuenta.Add(movimiento);
            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "CORE_TRANSACCION",
                Descripcion = System.Text.Json.JsonSerializer.Serialize(request),
                Servicio = "/core/transaction",
                Resultado = "OK"
            });

            return new CoreOperacionResponse
            {
                codigo = 0,
                descripcion = "Transacción aplicada exitosamente",
                saldo = saldoNuevo
            };
        }

        // SRV15 - Consultar saldo
        public async Task<CoreOperacionResponse> ConsultarSaldoAsync(CoreConsultaSaldoRequest request)
        {
            var cliente = await _context.ClientesBanco
                .FirstOrDefaultAsync(c => c.Identificacion == request.Identificacion);

            if (cliente == null)
            {
                return new CoreOperacionResponse
                {
                    codigo = -1,
                    descripcion = "Cliente no encontrado"
                };
            }

            var cuenta = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Numero_Cuenta == request.Cuenta &&
                                         c.Identificacion_Cliente == cliente.ID_Cliente);

            if (cuenta == null)
            {
                return new CoreOperacionResponse
                {
                    codigo = -1,
                    descripcion = "Cuenta no encontrada"
                };
            }

            return new CoreOperacionResponse
            {
                codigo = 0,
                descripcion = "Consulta exitosa",
                saldo = cuenta.Saldo
            };
        }

        // SRV16 - Consultar movimientos
        public async Task<List<MovimientoDTO>> ConsultarMovimientosAsync(CoreConsultaMovimientosRequest request)
        {
            var cliente = await _context.ClientesBanco
                .FirstOrDefaultAsync(c => c.Identificacion == request.Identificacion);

            if (cliente == null) return new List<MovimientoDTO>();

            var cuenta = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Numero_Cuenta == request.Cuenta &&
                                         c.Identificacion_Cliente == cliente.ID_Cliente);

            if (cuenta == null) return new List<MovimientoDTO>();

            var movimientos = await _context.MovimientosCuenta
                .Where(m => m.Numero_Cuenta == request.Cuenta)
                .OrderByDescending(m => m.FechaMovimiento)
                .Take(5)
                .Select(m => new MovimientoDTO
                {
                    Id = m.MovimientoId,
                    Fecha = m.FechaMovimiento,
                    Monto = m.Monto,
                    Tipo = m.TipoMovimiento,
                    Descripcion = m.Descripcion,
                    SaldoAnterior = m.SaldoAnterior ?? 0,
                    SaldoNuevo = m.SaldoNuevo ?? 0
                })
                .ToListAsync();

            return movimientos;
        }

        // SRV19 - Verificar cliente
        public async Task<bool> ClienteExisteAsync(string identificacion)
        {
            return await _context.ClientesBanco
                .AnyAsync(c => c.Identificacion == identificacion);
        }
    }
}
