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
            var respuesta = await _empresaProceso.CrearEmpresa(empresaDTO, ct);
            return respuesta;
        }

        [HttpGet("obtenerXIdCliente/{idCliente:int}")]
        public async Task<ActionResult<List<EmpresaDTO>>> ObtenerXIdCliente(int idCliente)
        {
            var lista = await _empresaProceso.ObtenerEmpresasXCliente(idCliente);
            return lista;
        }
    }
}
