using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios;
using API_SSO.Servicios.Contratos;

namespace API_SSO.Procesos
{
    public class LogProceso
    {
        private readonly ILogService<SSOContext> _service;

        public LogProceso(ILogService<SSOContext> service)
        {
            _service = service;
        }

        public async Task<LogDTO> CrearLog(LogDTO logDTO)
        {
            logDTO.EsSso = true;
            logDTO.Fecha = DateTime.Now;
            logDTO.Eliminado = false;
            var resultado = await _service.CrearYObtener(logDTO);
            return resultado;
        }
    }
}
