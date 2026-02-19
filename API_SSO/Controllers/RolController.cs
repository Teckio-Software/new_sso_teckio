using API_SSO.DTO;
using API_SSO.Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SSO.Controllers
{
    [Route("api/Rol")]
    [ApiController]
    public class RolController : ControllerBase
    {
        private readonly RolProceso _proceso;

        public RolController(RolProceso proceso)
        {
            _proceso = proceso;
        }

        [HttpPost("CrearRol")]
        [Authorize]
        public async Task<ActionResult<RolDTO>> CrearRol(RolCreacionDTO rol)
        {
            var authen = HttpContext.User;
            var resultado = await _proceso.CrearRol(rol, authen.Claims.ToList());
            return resultado;
        }

        [HttpPut("EditarRol")]
        [Authorize]
        public async Task<ActionResult<RespuestaDTO>> EditarRol(RolEdicionDTO rol)
        {
            var authen = HttpContext.User;
            var resultado = await _proceso.EditarRol(rol, authen.Claims.ToList());
            return resultado;
        }

        [HttpGet("ObtenerRolesXEmpresa/{idEmpresa:int}")]
        [Authorize]
        public async Task<ActionResult<List<RolDTO>>> ObtenerRolesXEmpresa(int idEmpresa)
        {
            var authen = HttpContext.User;
            var lista = await _proceso.ObtenerXEmpresa(idEmpresa, authen.Claims.ToList());
            return lista;
        }
    }
}
