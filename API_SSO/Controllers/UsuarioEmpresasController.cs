using API_SSO.DTO;
using API_SSO.Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static API_SSO.Controllers.ProyectoActualController;

namespace API_SSO.Controllers
{
    [Route("api/UsuarioEmpresa")]
    [ApiController]
    public class UsuarioEmpresasController : ControllerBase
    {
        private readonly UsuarioEmpresasProceso _proceso;

        public UsuarioEmpresasController(UsuarioEmpresasProceso proceso)
        {
            _proceso = proceso;
        }

        [HttpPost("ObtenerEmpresasXUsuario")]
        [Authorize]
        public async Task<ActionResult<List<EmpresaDTO>>> ObtenerEmpresasXUsuario(IdUsuario parametro)
        {
            var authen = HttpContext.User;
            var resultado = await _proceso.ObtenerEmpresasXUsuario(parametro.id, authen.Claims.ToList());
            return resultado;
        }

        [HttpGet("ObtenerEmpresasXPerteneciente")]
        [Authorize]
        public async Task<ActionResult<List<EmpresaDTO>>> ObtenerEmpresasPerteneciente()
        {
            var resultado = await _proceso.ObtenerEmpresasPerteneciente(HttpContext.User.Claims.ToList());
            return resultado;
        }

        [HttpPost("ObtenerEmpresasXUsuarioRelacion")]
        [Authorize]
        public async Task<ActionResult<List<RelacionEmpresaUsuarioDTO>>> ObtenerEmpresasPertenecienteXIdUsuario(IdUsuario parametro)
        {
            var resultado = await _proceso.ObtenerEmpresasPertenecientePorUsuario(HttpContext.User.Claims.ToList(), parametro.id);
            return resultado;
        }

        [HttpGet("ObtenerUsuariosXEmpresa/{idEmpresa:int}")]
        [Authorize]
        public async Task<ActionResult<List<UsuarioDTO>>> ObtenerUsuariosXEmpresa(int idEmpresa)
        {
            var resultado = await _proceso.ObtenerUsuariosXEmpresa(idEmpresa, HttpContext.User.Claims.ToList());
            return resultado;
        }

        [HttpPost("activarDesactivarEmpresaEnUsuario")]
        [Authorize]
        public async Task<ActionResult<RespuestaDTO>> ActivarDesactivarEmpresaEnUsuario(RelacionEmpresaUsuarioDTO parametro)
        {
            var authen = HttpContext.User;
            var resultado = await _proceso.ActivarDesactivarEmpresaEnUsuario(parametro, authen.Claims.ToList());
            return resultado;
        }
    }
}
