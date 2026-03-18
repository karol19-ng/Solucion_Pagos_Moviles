using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;

namespace API_Proyecto1.Controllers
{
   [ApiController]
    [Route("[controller]")]
    public class auth : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly IAfiliacionService _afiliacionService;

        public auth(ILoginService loginService, IAfiliacionService afiliacionService)
        {
            _loginService = loginService;
            _afiliacionService = afiliacionService;
        }


        // Endpoint solo para pruebas - generar contraseña encriptada
        //    [HttpGet("encrypt-test")]
        //   public IActionResult EncryptTest()
        //   {
        //       string password = "Ozzy.2025";
        //
        //        string hash = BCrypt.Net.BCrypt.HashPassword(password);

        //         return Ok(new
        //         {
        //             usuario = "marinnet",
        //             password_original = password,
        //             password_encriptada = hash
        //          });
        //      }
        // SRV5 - Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] System.Text.Json.JsonElement requestRaw)
        {
            try
            {
                // Convertimos el JSON crudo a nuestra clase manualmente
                var opciones = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var request = System.Text.Json.JsonSerializer.Deserialize<LoginRequest>(requestRaw.GetRawText(), opciones);

                if (request == null || string.IsNullOrWhiteSpace(request.usuario))
                {
                    return BadRequest(new { codigo = -1, descripcion = "Sigue llegando vacío: " + requestRaw.GetRawText() });
                }

                var result = await _loginService.LoginAsync(request);
                if (result == null) return Unauthorized(new { codigo = -1, descripcion = "Fallo de validación" });
                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { codigo = -1, descripcion = ex.Message });
            }
        }




        // SRV5 - Refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            try
            {
                var result = await _loginService.RefreshTokenAsync(request.refresh_token);
                return StatusCode(201, result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { codigo = -1, descripcion = "No autorizado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        // SRV5 - Validate
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateTokenRequest request)
        {
            try
            {
                var isValid = await _loginService.ValidateTokenAsync(request.token);
                if (isValid)
                    return Ok(true);
                else
                    return Unauthorized();
            }
            catch
            {
                return Unauthorized();
            }
        }

        // SRV9 - Register (Inscripción)
        [HttpPost("register")]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] AfiliacionRequest request)
        {
            try
            {
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _afiliacionService.InscribirAsync(request, usuario);

                if (result.Codigo == 0)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { codigo = -1, descripcion = ex.Message });
            }
        }

        // SRV10 - Cancel Subscription (Desinscripción)
        [HttpPost("cancel-subscription")]
        [Authorize]
        public async Task<IActionResult> CancelSubscription([FromBody] AfiliacionRequest request)
        {
            try
            {
                var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
                var result = await _afiliacionService.DesinscribirAsync(request, usuario);

                if (result.Codigo == 0)
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
