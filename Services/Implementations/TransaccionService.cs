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
using Microsoft.Extensions.Configuration;
using System.Net.Http;


namespace Services.Implementations
{
    public class TransaccionService : ITransaccionService
    {
        private readonly PagosMovilesDbContext _context;
        private readonly ICoreBancarioService _coreService;
        private readonly IBitacoraService _bitacoraService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public TransaccionService(
            PagosMovilesDbContext context,
            ICoreBancarioService coreService,
            IBitacoraService bitacoraService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _coreService = coreService;
            _bitacoraService = bitacoraService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // SRV7 - Recibir transacción
        public async Task<TransaccionResponse> RecibirTransaccionAsync(RecibirTransaccionRequest request, string usuarioEjecutor)
        {
            // Validaciones exactas del documento
            if (request.Entidad_origen <= 0 || request.Entidad_destino <= 0 ||
                string.IsNullOrWhiteSpace(request.Telefono_origen) ||
                string.IsNullOrWhiteSpace(request.Nombre_Origen) ||
                string.IsNullOrWhiteSpace(request.Telefono_destino) ||
                request.Monto <= 0 ||
                string.IsNullOrWhiteSpace(request.Descripcion))
            {
                return new TransaccionResponse
                {
                    codigo = -1,
                    descripcion = "Debe enviar los datos completos y válidos"
                };
            }

            if (request.Descripcion.Length > 25)
            {
                return new TransaccionResponse
                {
                    codigo = -1,
                    descripcion = "La descripción no puede superar 25 caracteres"
                };
            }

            if (request.Monto > 100000)
            {
                return new TransaccionResponse
                {
                    codigo = -1,
                    descripcion = "El monto no debe ser superior a 100.000"
                };
            }

            // Validar teléfono
            if (!IsValidPhone(request.Telefono_destino))
            {
                return new TransaccionResponse
                {
                    codigo = -1,
                    descripcion = "Debe enviar los datos completos y válidos"
                };
            }

            // Validar que entidad origen exista
            var entidadOrigen = await _context.Entidades.FindAsync(request.Entidad_origen);
            if (entidadOrigen == null)
            {
                return new TransaccionResponse
                {
                    codigo = -1,
                    descripcion = "Debe enviar los datos completos y válidos"
                };
            }

            // Llamar a SRV12 para ruteo
            var routeRequest = new RouteTransactionRequest
            {
                Telefono_origen = request.Telefono_origen,
                Nombre_Origen = request.Nombre_Origen,
                Telefono_destino = request.Telefono_destino,
                Monto = request.Monto,
                Descripcion = request.Descripcion
            };

            var routeResult = await RouteTransactionAsync(routeRequest, usuarioEjecutor);

            // Guardar transacción
            var transaccion = new TransaccionEnvio
            {
                ID_Entidad_Origen = request.Entidad_origen,
                ID_EntidadDestino = request.Entidad_destino,
                Telefono_Origen = request.Telefono_origen,
                Nombre_Origen = request.Nombre_Origen,
                Telefono_Destino = request.Telefono_destino,
                Monto = request.Monto,
                Descripcion = request.Descripcion,
                FechaEnvio = DateTime.Now,
                Codigo_Respuesta = routeResult.codigo,
                Mensaje_Respuesta = routeResult.descripcion,
                ID_Estado = routeResult.codigo == 0 ? 1 : 2
            };

            _context.TransaccionEnvios.Add(transaccion);
            await _context.SaveChangesAsync();

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "RECIBIR_TRANSACCION",
                Descripcion = System.Text.Json.JsonSerializer.Serialize(request),
                Servicio = "/transactions/process",
                Resultado = routeResult.codigo == 0 ? "OK" : "ERROR"
            });

            return routeResult;
        }

        // SRV8 - Enviar transacción
        public async Task<TransaccionResponse> EnviarTransaccionAsync(EnviarTransaccionRequest request, string usuarioEjecutor)
        {
            // Validaciones
            if (request.Entidad_origen <= 0 ||
                string.IsNullOrWhiteSpace(request.Telefono_origen) ||
                string.IsNullOrWhiteSpace(request.Nombre_Origen) ||
                string.IsNullOrWhiteSpace(request.Telefono_destino) ||
                request.Monto <= 0 ||
                string.IsNullOrWhiteSpace(request.Descripcion))
            {
                return new TransaccionResponse
                {
                    codigo = -1,
                    descripcion = "Debe enviar los datos completos y válidos"
                };
            }

            if (request.Descripcion.Length > 25)
            {
                return new TransaccionResponse
                {
                    codigo = -1,
                    descripcion = "La descripción no puede superar 25 caracteres"
                };
            }

            if (request.Monto > 100000)
            {
                return new TransaccionResponse
                {
                    codigo = -1,
                    descripcion = "El monto no debe ser superior a 100.000"
                };
            }

            // Aquí se enviaría a la entidad externa
            // Como es simulación, retornamos éxito
            var response = new TransaccionResponse
            {
                codigo = 0,
                descripcion = "Transacción enviada exitosamente"
            };

            await _bitacoraService.RegistrarBitacoraAsync(new BitacoraRegistroRequest
            {
                Usuario = usuarioEjecutor,
                Accion = "ENVIAR_TRANSACCION",
                Descripcion = System.Text.Json.JsonSerializer.Serialize(request),
                Servicio = "/transactions/send",
                Resultado = "OK"
            });

            return response;
        }

        // SRV12 - Ruteo de transacciones
        public async Task<TransaccionResponse> RouteTransactionAsync(RouteTransactionRequest request, string usuarioEjecutor)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(request.Telefono_origen) ||
                string.IsNullOrWhiteSpace(request.Nombre_Origen) ||
                string.IsNullOrWhiteSpace(request.Telefono_destino) ||
                request.Monto <= 0 ||
                string.IsNullOrWhiteSpace(request.Descripcion))
            {
                return new TransaccionResponse
                {
                    codigo = -1,
                    descripcion = "Debe enviar los datos completos y válidos"
                };
            }

            if (request.Descripcion.Length > 25)
            {
                return new TransaccionResponse
                {
                    codigo = -1,
                    descripcion = "La descripción no puede superar 25 caracteres"
                };
            }

            if (request.Monto > 100000)
            {
                return new TransaccionResponse
                {
                    codigo = -1,
                    descripcion = "El monto no debe ser superior a 100.000"
                };
            }

            // Validar que teléfono origen esté afiliado
            var afiliacionOrigen = await _context.Afiliaciones
                .FirstOrDefaultAsync(a => a.Telefono == request.Telefono_origen && a.ID_Estado == 4); // Habilitado

            if (afiliacionOrigen == null)
            {
                return new TransaccionResponse
                {
                    codigo = -1,
                    descripcion = "Cliente no asociado a pagos móviles"
                };
            }

            // Verificar si teléfono destino es interno
            var afiliacionDestino = await _context.Afiliaciones
                .FirstOrDefaultAsync(a => a.Telefono == request.Telefono_destino && a.ID_Estado == 4);

            if (afiliacionDestino != null)
            {
                // Transacción interna - Usar SRV14 para crédito
                var coreRequest = new CoreTransaccionRequest
                {
                    Identificacion = afiliacionDestino.Identificacion_Usuario,
                    Tipo_Movimiento = "CREDITO",
                    Monto = request.Monto
                };

                var coreResult = await _coreService.AplicarTransaccionAsync(coreRequest, usuarioEjecutor);

                return new TransaccionResponse
                {
                    codigo = coreResult.codigo,
                    descripcion = coreResult.codigo == 0 ? "Transacción aplicada" : coreResult.descripcion
                };
            }
            else
            {
                // Transacción externa - Usar SRV8
                // Aquí iría la llamada HTTP a la entidad externa
                return new TransaccionResponse
                {
                    codigo = 0,
                    descripcion = "Transacción enviada a entidad externa"
                };
            }
        }

        private bool IsValidPhone(string phone)
        {
            return Regex.IsMatch(phone, @"^\d{8}$"); // Formato Costa Rica
        }
    }
}