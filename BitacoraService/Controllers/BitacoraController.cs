// BitacoraService/Controllers/BitacoraController.cs (SRV18)
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace BitacoraService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class bitacora : ControllerBase
    {
        private readonly IBitacoraService _service;

        public bitacora(IBitacoraService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] BitacoraRegistroRequest request)
        {
            try
            {
                await _service.RegistrarBitacoraAsync(request);
                return StatusCode(201);
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

        [HttpGet]
        public async Task<ActionResult<List<Bitacoratransaccionresponse>>> GetByFecha(
            [FromQuery] DateTime? fecha)
        {
            try
            {
                var result = await _service.ConsultarTransaccionesAsync(fecha);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        [HttpGet("usuario/{usuario}")]
        public async Task<ActionResult<List<BitacoraResponse>>> GetByUsuario(string usuario)
        {
            try
            {
                var result = await _service.ConsultarPorUsuarioAsync(usuario);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }
    }
}