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
    public class rol : ControllerBase
    {
        private readonly IRolService _service;

        public rol(IRolService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<RolResponse>>> GetAll()
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
        public async Task<ActionResult<RolResponse>> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null) return NotFound(new { codigo = -1, descripcion = "Rol no encontrado" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<RolResponse>> Create([FromBody] RolRequest request)
        {
            try
            {
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.CreateAsync(request, usuario);
                return CreatedAtAction(nameof(GetById), new { id = result.ID_Rol }, result);
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
        public async Task<ActionResult<RolResponse>> Update(int id, [FromBody] RolRequest request)
        {
            try
            {
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.UpdateAsync(id, request, usuario);
                if (result == null) return NotFound(new { codigo = -1, descripcion = "Rol no encontrado" });
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
                if (!result) return NotFound(new { codigo = -1, descripcion = "Rol no encontrado" });
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }
    }
}
