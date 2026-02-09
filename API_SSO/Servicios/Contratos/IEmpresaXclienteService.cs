using API_SSO.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios.Contratos
{
    public interface IEmpresaXclienteService<TContext> where TContext : DbContext
    {
        Task<List<EmpresaXclienteDTO>> ObtenerTodos();
        Task<List<EmpresaXclienteDTO>> ObtenerPorIdCliente(int idCliente);
        Task<EmpresaXclienteDTO> ObtenerXId(int id);
        Task<EmpresaXclienteDTO> CrearYObtener(EmpresaXclienteDTO empresaXcliente);
        Task<RespuestaDTO> Editar(EmpresaXclienteDTO empresaXcliente);
        Task<RespuestaDTO> Eliminar(int id);

    }
}
