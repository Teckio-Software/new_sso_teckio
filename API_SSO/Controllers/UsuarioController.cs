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
        public async Task EnviarEmailRecuperacion([FromBody] EnviarEmailRecuperacionDTO enviarEmailRecuperacionDTO, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(enviarEmailRecuperacionDTO.Email))
            {
                //return BadRequest(new { ok = false, message = "Email requerido" });
                return;
            }

            await _usuarioProceso.EnviarEmailRecuperacion(enviarEmailRecuperacionDTO.Email, ct);
            return;
        }


        [HttpPost("restablecerContrasena")]
        [Authorize]
        public async Task<ActionResult<RespuestaDTO>> RestablecerContrasena(RecuperacionContrasenaDTO objeto)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            var authen = HttpContext.User;
            var resultado = await _usuarioProceso.RestablecerContrasena(objeto, authen.Claims.ToList(), authHeader);
            return resultado;
        }

    }
}
