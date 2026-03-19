using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;

namespace API_Proyecto1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class screen : ControllerBase
    {
        private readonly IPantallaService _service;
        private readonly ILogger<screen> _logger;  // Agregar logger

        public screen(IPantallaService service, ILogger<screen> logger)  // Inyectar logger
        {
            _service = service;
            _logger = logger;
        }


        [HttpGet]
        public async Task<ActionResult<List<PantallaResponse>>> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PantallaResponse>> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null) return NotFound(new { codigo = -1, descripcion = "Pantalla no encontrada" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PantallaResponse>> Create([FromBody] PantallaRequest request)
        {
            try
            {
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.CreateAsync(request, usuario);
                return CreatedAtAction(nameof(GetById), new { id = result.ID_Pantalla }, result);
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

        [HttpPut("{id}")]
        public async Task<ActionResult<PantallaResponse>> Update(int id, [FromBody] PantallaRequest request)
        {
            try
            {
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.UpdateAsync(id, request, usuario);
                if (result == null) return NotFound(new { codigo = -1, descripcion = "Pantalla no encontrada" });
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("=== RECIBIDA PETICIÓN DELETE EN API ===");
                _logger.LogInformation("ID a eliminar: {Id}", id);

                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                _logger.LogInformation("Usuario: {Usuario}", usuario);

                var result = await _service.DeleteAsync(id, usuario);

                if (!result)
                {
                    return NotFound(new { codigo = -1, descripcion = "Pantalla no encontrada" });
                }

                _logger.LogInformation("✅ Pantalla {Id} eliminada exitosamente", id);
                return Ok(new { codigo = 0, descripcion = "Pantalla eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al eliminar pantalla {Id}", id);
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }
    }
}