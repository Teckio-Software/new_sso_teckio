using API_SSO.DTO;
using API_SSO.Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_SSO.Controllers
{

    [Route("api/usuario")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioProceso _usuarioProceso;

        public UsuarioController(UsuarioProceso usuarioProceso)
        {
            _usuarioProceso = usuarioProceso;
        }

        [HttpPost("login")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login([FromBody] CredencialesUsuarioDTO credenciales)
        {
            return await _usuarioProceso.Login(credenciales);
        }

        [HttpPost("asignarRol")]
        public async Task<ActionResult<RespuestaDTO>> AsignarRol(UsuarioRolDTO rol)
        {
            var resultado = await _usuarioProceso.AsignarRol(rol);
            return resultado;
        }

        public record EnviarEmailRecuperacionDTO(string Email);

        [HttpPost("enviarEmailRecuperacion")]
        [AllowAnonymous]
        public async Task<IActionResult> EnviarEmailRecuperacion([FromBody] EnviarEmailRecuperacionDTO enviarEmailRecuperacionDTO, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(enviarEmailRecuperacionDTO.Email))
            {
                return BadRequest(new { ok = false, message = "Email requerido" });
            }

            await _usuarioProceso.EnviarEmailRecuperacion(enviarEmailRecuperacionDTO.Email, ct);
            return Ok(new { ok = true });
        }


        [HttpPost("restablecerContrasena")]
        [AllowAnonymous]
        public async Task<ActionResult<RespuestaDTO>> RestablecerContrasena(RecuperacionContrasenaDTO objeto)
        {
            var resultado = await _usuarioProceso.RestablecerContrasena(objeto);
            return resultado;
        }

        public record ValidateResetTokenRequest(string Email, string Token);

        [HttpPost]
        [Route("validate-token")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidarToken([FromBody] ValidateResetTokenRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Token))
            {
                return BadRequest(new { ok = false, message = "Email y token requeridos" });
            }

            var resultado = await _usuarioProceso.ValidarToken(req.Email, req.Token);
            return Ok(new { ok = resultado });
        }

    }
}
