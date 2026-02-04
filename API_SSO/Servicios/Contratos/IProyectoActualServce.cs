using API_SSO.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios.Contratos
{
    public interface IProyectoActualServce<TContext> where TContext : DbContext
    {
        Task<List<ProyectoActualDTO>> ObtenerTodos();
        Task<ProyectoActualDTO> ObtenerXId(int id);
        Task<ProyectoActualDTO> ObtenerXIdUsuario(string id);
        Task<ProyectoActualDTO> CrearYObtener(ProyectoActualDTO proyectoActual);
        Task<RespuestaDTO> Editar(ProyectoActualDTO proyectoActual);
        Task<RespuestaDTO> Eliminar(int id);

    }
}
