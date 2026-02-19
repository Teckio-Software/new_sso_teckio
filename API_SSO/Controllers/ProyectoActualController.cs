using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Procesos;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SSO.Controllers
{

    [Route("api/ProyectoActual")]
    [ApiController]
    public class ProyectoActualController : ControllerBase
    {
        private readonly IProyectoActualServce<SSOContext> _proyectoActualServce;
        private readonly LogProceso _logProceso;

        public ProyectoActualController(IProyectoActualServce<SSOContext> proyectoActualServce, LogProceso logProceso)
        {
            _proyectoActualServce = proyectoActualServce;
            _logProceso = logProceso;
        }

        public record IdUsuario(string id);
        [HttpPost("ObtenerXIdUsuario")]
        [Authorize]
        public async Task<ActionResult<ProyectoActualDTO>> ObtenerXIdUsuario(IdUsuario parametro)
        {
            var authen = HttpContext.User;
            var idUs = authen.Claims.First(c => c.Type == "guid")?.Value;
            if (idUs == null)
            {
                return new ProyectoActualDTO();
            }
            var respuesta = await _proyectoActualServce.ObtenerXIdUsuario(parametro.id);
            if (respuesta.Id > 0)
            {
                await _logProceso.CrearLog(idUs, "Controlador", "ProyectoActualController", $"Se obtuvo el proyecto actual del usuario {parametro.id}");
            }
            else
            {
                await _logProceso.CrearLog(idUs, "Controlador", "ProyectoActualController", $"No se encontró el proyecto actual del usuario {parametro.id}");
            }
            return respuesta;
        }

        [HttpPost("CrearProyectoActual")]
        [Authorize]
        public async Task<ActionResult<ProyectoActualDTO>> CrearProyectoActual([FromBody] ProyectoActualDTO parametro)
        {
            var authen = HttpContext.User;
            var idUs = authen.Claims.First(c => c.Type == "guid")?.Value;
            if (idUs == null)
            {
                return new ProyectoActualDTO();
            }
            var respuesta = await _proyectoActualServce.CrearYObtener(parametro);
            if (respuesta.Id > 0)
            {
                await _logProceso.CrearLog(idUs, "Controlador", "ProyectoActualController", $"Se creó el proyecto actual del usuario {parametro.UserId}");
            }
            else
            {
                await _logProceso.CrearLog(idUs, "Controlador", "ProyectoActualController", $"No se pudo crear el proyecto actual del usuario {parametro.UserId}");
            }
            return respuesta;
        }

        [HttpPost("EditarProyectoActual")]
        [Authorize]
        public async Task<ActionResult<RespuestaDTO>> EditarProyectoActual([FromBody] ProyectoActualDTO parametro)
        {
            var authen = HttpContext.User;
            var idUs = authen.Claims.First(c => c.Type == "guid")?.Value;
            if (idUs == null)
            {
                return new RespuestaDTO
                {
                    Estatus = false,
                    Descripcion = "Usuario no autenticado."
                };
            }
            var respuesta = await _proyectoActualServce.Editar(parametro);
            if(respuesta.Estatus)
            {
                await _logProceso.CrearLog(idUs, "Controlador", "ProyectoActualController", $"Se editó el proyecto actual del usuario {parametro.UserId}");
            }
            else
            {
                await _logProceso.CrearLog(idUs, "Controlador", "ProyectoActualController", $"No se pudo editar el proyecto actual del usuario {parametro.UserId}");
            }
            return respuesta;
        }
    }
}
