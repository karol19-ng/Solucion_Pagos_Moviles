using Pegasos.Web.Administrador.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Pegasos.Web.Administrador.Services;
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

        // SA12 - Reporte diario
        [HttpGet("reporte")]
        public async Task<IActionResult> Reporte([FromQuery] DateTime fecha)
        {
            try
            {
                if (fecha == default)
                    return BadRequest(new { codigo = -1, descripcion = "Debe indicar una fecha válida. Ejemplo: ?fecha=2025-04-14" });

                var result = await _service.ConsultarTransaccionesPorFechaAsync(fecha);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }
    }
}