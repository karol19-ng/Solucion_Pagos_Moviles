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
        private readonly ILogger<rol> _logger;  // Agregar logger

        public rol(IRolService service, ILogger<rol> logger)  // Inyectar logger
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<RolResponse>>> GetAll()
        {
            try
            {
                _logger.LogInformation("=== GET ALL ROLES ===");
                var result = await _service.GetAllAsync();
                _logger.LogInformation("Roles encontrados: {Count}", result?.Count ?? 0);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAll");
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RolResponse>> GetById(int id)
        {
            try
            {
                _logger.LogInformation("=== GET ROL BY ID {Id} ===", id);
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Rol {Id} no encontrado", id);
                    return NotFound(new { codigo = -1, descripcion = "Rol no encontrado" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetById para {Id}", id);
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<RolResponse>> Create([FromBody] RolRequest request)
        {
            try
            {
                _logger.LogInformation("=== CREATE ROL EN API ===");
                _logger.LogInformation("Request recibido: {@Request}", request);
                _logger.LogInformation("Nombre: {Nombre}", request.Nombre);
                _logger.LogInformation("Pantallas: {Pantallas}", string.Join(",", request.Pantallas));

                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.CreateAsync(request, usuario);

                _logger.LogInformation("Rol creado con ID: {Id}", result.ID_Rol);
                return CreatedAtAction(nameof(GetById), new { id = result.ID_Rol }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("ArgumentException: {Message}", ex.Message);
                return BadRequest(new { codigo = -1, descripcion = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Create: {Message}", ex.Message);
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RolResponse>> Update(int id, [FromBody] RolRequest request)
        {
            try
            {
                _logger.LogInformation("=== UPDATE ROL {Id} ===", id);
                _logger.LogInformation("Request: {@Request}", request);

                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.UpdateAsync(id, request, usuario);

                if (result == null)
                {
                    _logger.LogWarning("Rol {Id} no encontrado", id);
                    return NotFound(new { codigo = -1, descripcion = "Rol no encontrado" });
                }

                _logger.LogInformation("Rol {Id} actualizado exitosamente", id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("ArgumentException: {Message}", ex.Message);
                return BadRequest(new { codigo = -1, descripcion = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Update para {Id}", id);
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("=== DELETE ROL {Id} ===", id);

                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _service.DeleteAsync(id, usuario);

                if (!result)
                {
                    _logger.LogWarning("Rol {Id} no encontrado", id);
                    return NotFound(new { codigo = -1, descripcion = "Rol no encontrado" });
                }

                _logger.LogInformation("Rol {Id} eliminado exitosamente", id);
                return Ok(new { codigo = 0, descripcion = "Rol eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Delete para {Id}", id);
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }
    }
}