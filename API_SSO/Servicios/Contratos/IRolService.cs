using API_SSO.DTO;
using API_SSO.DTOs;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios.Contratos
{
    public interface IRolService<TContext> where TContext : DbContext
    {
        Task<List<RolDTO>> ObtenerTodos();
        Task<RolDTO> ObtenerXId(int id);
        Task<RolDTO> CrearYObtener(RolDTO rol);
        Task<RespuestaDTO> Editar(RolDTO rol);
        Task<RespuestaDTO> Eliminar(int id);
    }
}
