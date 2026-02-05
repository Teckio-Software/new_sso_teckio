using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;

namespace API_SSO.Controllers
{

    [Route("api/ProyectoActual")]
    [ApiController]
    public class ProyectoActualController : ControllerBase
    {
        private readonly IProyectoActualServce<SSOContext> _proyectoActualServce;

        public ProyectoActualController(IProyectoActualServce<SSOContext> proyectoActualServce)
        {
            _proyectoActualServce = proyectoActualServce;
        }

        public record IdUsuario(string id);
        [HttpPost("ObtenerXIdUsuario")]
        public async Task<ActionResult<ProyectoActualDTO>> ObtenerXIdUsuario(IdUsuario parametro)
        {
            var respuesta = await _proyectoActualServce.ObtenerXIdUsuario(parametro.id);
            return respuesta;
        }

        [HttpPost("CrearProyectoActual")]
        public async Task<ActionResult<ProyectoActualDTO>> CrearProyectoActual([FromBody] ProyectoActualDTO parametro)
        {
            var respuesta = await _proyectoActualServce.CrearYObtener(parametro);
            return respuesta;
        }

        [HttpPost("EditarProyectoActual")]
        public async Task<ActionResult<RespuestaDTO>> EditarProyectoActual([FromBody] ProyectoActualDTO parametro)
        {
            var respuesta = await _proyectoActualServce.Editar(parametro);
            return respuesta;
        }
    }
}
