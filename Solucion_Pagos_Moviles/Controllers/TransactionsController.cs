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
    public class transactions : ControllerBase
    {
        private readonly ITransaccionService _service;

        public transactions(ITransaccionService service)
        {
            _service = service;
        }

        // SRV7 - Recibir transacción
        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] RecibirTransaccionRequest request)
        {
            try
            {
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.RecibirTransaccionAsync(request, usuario);

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

        // SRV8 - Enviar transacción
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] EnviarTransaccionRequest request)
        {
            try
            {
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.EnviarTransaccionAsync(request, usuario);

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

        // SRV12 - Ruteo
        [HttpPost("route")]
        public async Task<IActionResult> Route([FromBody] RouteTransactionRequest request)
        {
            try
            {
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.RouteTransactionAsync(request, usuario);

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
    }
}
