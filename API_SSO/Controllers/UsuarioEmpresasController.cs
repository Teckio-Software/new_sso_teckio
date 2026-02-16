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
            var resultado = await _proceso.ObtenerEmpresasXUsuario(parametro.id);
            return resultado;
        }

        [HttpGet("ObtenerEmpresasXPerteneciente")]
        [Authorize]
        public async Task<ActionResult<List<EmpresaDTO>>> ObtenerEmpresasPerteneciente()
        {
            //var zvUsernameClaim = HttpContext.User.Claims.FirstOrDefault()!.Value;
            var resultado = await _proceso.ObtenerEmpresasPerteneciente(HttpContext.User.Claims.ToList());
            return resultado;
        }
    }
}
