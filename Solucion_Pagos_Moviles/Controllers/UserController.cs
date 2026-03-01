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
    public class user : ControllerBase
    {
        private readonly IUsuarioService _service;

        public user(IUsuarioService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<UsuarioResponse>>> GetAll()
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
        public async Task<ActionResult<UsuarioResponse>> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null) return NotFound(new { codigo = -1, descripcion = "Usuario no encontrado" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<UsuarioResponse>>> GetByFilter(
            [FromQuery] string identificacion,
            [FromQuery] string nombre,
            [FromQuery] string tipo)
        {
            try
            {
                var result = await _service.GetByFilterAsync(identificacion, nombre, tipo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<UsuarioResponse>> Create([FromBody] UsuarioRequest request)
        {
            try
            {
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.CreateAsync(request, usuario);
                return CreatedAtAction(nameof(GetById), new { id = result.ID_Usuario }, result);
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
        public async Task<ActionResult<UsuarioResponse>> Update(int id, [FromBody] UsuarioRequest request)
        {
            try
            {
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.UpdateAsync(id, request, usuario);
                if (result == null) return NotFound(new { codigo = -1, descripcion = "Usuario no encontrado" });
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
                if (!result) return NotFound(new { codigo = -1, descripcion = "Usuario no encontrado" });
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }
    }
}