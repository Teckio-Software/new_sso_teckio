using API_SSO.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios.Contratos
{
    public interface ILogService<TContext> where TContext : DbContext
    {
        Task<List<LogDTO>> ObtenerTodos();
        Task<List<LogDTO>> ObtenerXEmpresa(int id);
        Task<LogDTO> ObtenerXId(int id);
        Task<LogDTO> CrearYObtener(LogDTO log);
        Task<RespuestaDTO> Editar(LogDTO log);
        Task<RespuestaDTO> Eliminar(int id);
    }
}
