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
    public class core : ControllerBase
    {
        private readonly ICoreBancarioService _service;

        public core(ICoreBancarioService service)
        {
            _service = service;
        }

        // SRV14 - Transacción
        [HttpPost("transaction")]
        public async Task<IActionResult> Transaction([FromBody] CoreTransaccionRequest request)
        {
            try
            {
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.AplicarTransaccionAsync(request, usuario);

                if (result.codigo == 0)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        // SRV15 - Consultar saldo
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance([FromQuery] string identificacion, [FromQuery] string cuenta)
        {
            try
            {
                var request = new CoreConsultaSaldoRequest
                {
                    Identificacion = identificacion,
                    Cuenta = cuenta
                };

                var result = await _service.ConsultarSaldoAsync(request);

                if (result.codigo == 0)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        // SRV16 - Consultar movimientos
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] string identificacion, [FromQuery] string cuenta)
        {
            try
            {
                var request = new CoreConsultaMovimientosRequest
                {
                    Identificacion = identificacion,
                    Cuenta = cuenta
                };

                var result = await _service.ConsultarMovimientosAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        // SRV19 - Verificar cliente
        [HttpGet("client-exists")]
        public async Task<IActionResult> ClientExists([FromQuery] string identificacion)
        {
            try
            {
                var result = await _service.ClienteExisteAsync(identificacion);
                return Ok(new { Existe = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }
    }
}