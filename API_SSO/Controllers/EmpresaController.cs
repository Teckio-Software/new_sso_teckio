using API_SSO.DTO;
using API_SSO.Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SSO.Controllers
{
    [Route("api/empresa")]
    [ApiController]
    public class EmpresaController : ControllerBase
    {
        private readonly EmpresaProceso _empresaProceso;

        public EmpresaController(EmpresaProceso empresaProceso)
        {
            _empresaProceso = empresaProceso;
        }

        [HttpPost("crearEmpresa")]
        [Authorize]
        public async Task<ActionResult<RespuestaDTO>> CrearEmpresa([FromBody] EmpresaCreacionDTO empresaDTO, CancellationToken ct)
        {
            var authen = HttpContext.User;
            var respuesta = await _empresaProceso.CrearEmpresa(empresaDTO, authen.Claims.ToList(), ct);
            return respuesta;
        }

        [HttpGet("obtenerXIdCliente/{idCliente:int}")]
        public async Task<ActionResult<List<EmpresaDTO>>> ObtenerXIdCliente(int idCliente)
        {
            var authen = HttpContext.User;
            var lista = await _empresaProceso.ObtenerEmpresasXCliente(idCliente, authen.Claims.ToList());
            return lista;
        }

        [HttpPost("editarEmpresa")]
        public async Task<ActionResult<RespuestaDTO>> EditarEmpresa(EmpresaDTO empresa)
        {
            var authen = HttpContext.User;
            var resultado = await _empresaProceso.EditarEmpresa(empresa, authen.Claims.ToList());
            return resultado;
        }
    }
}
