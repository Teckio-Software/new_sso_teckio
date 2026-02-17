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

        public async Task<LogDTO> CrearLog(string userId, string nivel, string metodo, string descripcion)
        {
            LogDTO logDTO = new LogDTO();
            logDTO.EsSso = true;
            logDTO.Fecha = DateTime.Now;
            logDTO.Eliminado = false;
            logDTO.IdEmpresa = null;
            logDTO.UserId = userId;
            logDTO.Nivel = nivel;
            logDTO.Metodo = metodo;
            logDTO.Descripcion = descripcion;
            var resultado = await _service.CrearYObtener(logDTO);
            return resultado;
        }
    }
}
