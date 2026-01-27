using API_SSO.DTO;
using API_SSO.Procesos;
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
        
    }
}
