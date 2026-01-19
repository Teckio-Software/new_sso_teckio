using API_SSO.DTO;
using API_SSO.DTOs;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios.Contratos
{
    public interface IClienteService<TContext> where TContext : DbContext
    {
        Task<List<ClienteDTO>> ObtenerTodos();
        Task<ClienteDTO> ObtenerXId(int id);
        Task<ClienteDTO> CrearYObtener(ClienteDTO clienteDTO);
        Task<RespuestaDTO> Editar(ClienteDTO clienteDTO);
        Task<RespuestaDTO> Eliminar(int id);
    }
}
