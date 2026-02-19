using API_SSO.DTO;
using API_SSO.Procesos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static API_SSO.Controllers.ProyectoActualController;

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
            var authen = HttpContext.User;
            var resultado = await _usuarioProceso.AsignarRol(rol, authen.Claims.ToList());
            return resultado;
        }

        public record EnviarEmailRecuperacionDTO(string Email);

        [HttpPost("enviarEmailRecuperacion")]
        [AllowAnonymous]
        public async Task<IActionResult> EnviarEmailRecuperacion([FromBody] EnviarEmailRecuperacionDTO enviarEmailRecuperacionDTO, CancellationToken ct)
        {
            var authen = HttpContext.User;
            if (string.IsNullOrWhiteSpace(enviarEmailRecuperacionDTO.Email))
            {
                return BadRequest(new { ok = false, message = "Email requerido" });
            }
            await _usuarioProceso.EnviarEmailRecuperacion(enviarEmailRecuperacionDTO.Email, ct);
            return Ok(new { ok = true });
        }


        [HttpPost("restablecerContrasena")]
        [AllowAnonymous]
        public async Task<ActionResult<RespuestaDTO>> RestablecerContrasena(RecuperacionContrasenaDTO objeto, CancellationToken ct)
        {
            var authen = HttpContext.User;
            var resultado = await _usuarioProceso.RestablecerContrasena(objeto, ct);
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

        public record ValidarUseroEmail(string parametro);
        [HttpPost("validarEmail")]
        public async Task<ActionResult<RespuestaDTO>> ValidarCorreo(ValidarUseroEmail parametro)
        {
            var respuesta = await _usuarioProceso.ValidarCorreo(parametro.parametro);
            return respuesta;
        }

        [HttpPost("validarNombreUsuario")]
        public async Task<ActionResult<RespuestaDTO>> ValidarNombreUsuario(ValidarUseroEmail parametro)
        {
            var respuesta = await _usuarioProceso.ValidarNombreUsuario(parametro.parametro);
            return respuesta;
        }

        [HttpGet("actualizarClaims")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<ActionResult<RespuestaAutenticacionDTO>> ActualizarClaims()
        {
            var zCredenciales = new CredencialesUsuarioDTO();
            var authen = HttpContext.User;
            var usernameClaim = authen.Claims.FirstOrDefault()!.Value;
            if (usernameClaim == null)
            {
                RespuestaAutenticacionDTO resp = new RespuestaAutenticacionDTO();
                resp.FechaExpiracion = DateTime.Today;
                resp.Token = "NoToken";
                return resp;
            }
            zCredenciales.Email = usernameClaim;
            return await _usuarioProceso.ConstruirToken(zCredenciales);
        }

        [HttpPost("obtenUsuario")]
        public async Task<ActionResult<UsuarioDTO>> obtenUsuario(IdUsuario parametros)
        {
            var authen = HttpContext.User;
            var resultado = await _usuarioProceso.ObtenerUsuarioXId(parametros.id, authen.Claims.ToList());
            return resultado;
        }

        [HttpPost("reestableceContrasenia")]
        public async Task<ActionResult<RespuestaDTO>> CambiarContrasenia(CambiarContraseniaDTO objeto)
        {
            var respuesta = await _usuarioProceso.ReestableceContrasenia(objeto, HttpContext.User.Claims.ToList());
            return respuesta;
        }
    }
}
