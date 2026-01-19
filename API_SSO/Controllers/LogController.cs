using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SSO.Controllers
{
    [Route("api/Logs")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogService<SSOContext> _logService;

        public LogController(ILogService<SSOContext> logService) {
            _logService = logService;
        }

        [HttpGet("ObtenerLogsEmpresa/{idempresa:int}")]
        public async Task<List<LogDTO>> ObtenerLogsXEmpresa(int idempresa)
        {
            var lista = await _logService.ObtenerXEmpresa(idempresa);
            return lista;
        }

        [HttpPost("CrearLog")]
        public async Task<LogDTO> CrearLog(LogDTO logDTO)
        {
            var authen = HttpContext.User;
            var IdUsStr = authen.Claims.Where(z => z.Type == "idUsuario").ToList();
            if (IdUsStr[0].Value == null)
            {
                return new LogDTO();
            }
            var IdUsuario = IdUsStr[0].Value;
            logDTO.UserId = IdUsuario;
            logDTO.Fecha = DateTime.Now;
            logDTO.Eliminado = false;
            var resultado = await _logService.CrearYObtener(logDTO);
            return resultado;
        }
    }
}
