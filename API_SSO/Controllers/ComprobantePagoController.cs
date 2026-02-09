using API_SSO.DTO;
using API_SSO.Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SSO.Controllers
{

    [ApiController]
    [Route("api/comprobantePago")]
    public class ComprobantePagoController : ControllerBase
    {
        private readonly ComprobantePagoProceso _proceso;

        public ComprobantePagoController(ComprobantePagoProceso proceso)
        {
            _proceso = proceso;
        }

        //public record ComprobanteConIdCliente(IFormFile archivo, int idCliente);

        //[HttpPost("subirComprobante")]
        //public async Task<ActionResult<RespuestaDTO>> subirComprobante(ComprobanteConIdCliente comprobante)
        //{
        //    var authen = HttpContext.User;
        //    var resultado = await _proceso.SubirComprobante(comprobante.archivo, comprobante.idCliente, authen.Claims.ToList());
        //    return resultado;
        //}
        [HttpGet("todos")]
        [Authorize]
        public async Task<ActionResult<List<ComprobantePagoDTO>>> ObtenerTodos()
        {
            var lista = await _proceso.ObtenerTodos();
            return lista;
        }

        [HttpGet("obtenerXIdCliente/{idCliente:int}")]
        [Authorize]
        public async Task<ActionResult<List<ComprobantePagoDTO>>> ObtenerXIdCliente(int idCliente)
        {
            var lista = await _proceso.ObtenerXIdCliente(idCliente);
            return lista;
        }

        [HttpGet("cancelarComprobante/{idComprobante:int}")]
        [Authorize]
        public async Task<ActionResult<RespuestaDTO>> CancelarComprobante(int idComprobante)
        {
            var resultado = await _proceso.CancelaComprobantePago(idComprobante);
            return resultado;  
        }

        [HttpGet("autorizarComprobante/{idComprobante:int}")]
        [Authorize]
        public async Task<ActionResult<RespuestaDTO>> AutorizarComprobante(int idComprobante, CancellationToken ct)
        {
            var authen = HttpContext.User;
            var resultado = await _proceso.AutorizarComprobantePago(idComprobante, authen.Claims.ToList(), ct);
            return resultado;
        }
    }
}
