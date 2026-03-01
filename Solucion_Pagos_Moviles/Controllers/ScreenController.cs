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
    public class screen : ControllerBase
    {
        private readonly IPantallaService _service;

        public screen(IPantallaService service)
        {
            _service = service;
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
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.DeleteAsync(id, usuario);
                if (!result) return NotFound(new { codigo = -1, descripcion = "Pantalla no encontrada" });
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }
    }
}