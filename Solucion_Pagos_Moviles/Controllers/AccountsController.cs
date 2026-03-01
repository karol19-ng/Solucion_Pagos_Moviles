// API_Proyecto1/Controllers/AccountsController.cs (SRV11, SRV13)
using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;

namespace API_Proyecto1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class accounts : ControllerBase
    {
        private readonly IAfiliacionService _afiliacionService;  // Nombre específico

        // Constructor con parámetro nombrado específicamente
        public accounts(IAfiliacionService afiliacionService)
        {
            _afiliacionService = afiliacionService;
        }

        // SRV11 - Últimos movimientos
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] string telefono, [FromQuery] string identificacion)
        {
            try
            {
                var request = new UltimosMovimientosRequest
                {
                    Telefono = telefono,
                    Identificacion = identificacion
                };

                var result = await _afiliacionService.ConsultarMovimientosAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { codigo = -1, descripcion = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        // SRV13 - Consultar saldo
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance([FromQuery] string telefono, [FromQuery] string identificacion)
        {
            try
            {
                var request = new ConsultaSaldoRequest
                {
                    Telefono = telefono,
                    Identificacion = identificacion
                };

                var result = await _afiliacionService.ConsultarSaldoAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { codigo = -1, descripcion = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }
    }
}