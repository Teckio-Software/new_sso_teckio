using API_SSO.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios.Contratos
{
    public interface IEmpresaService<TContext> where TContext : DbContext
    {
        Task<List<EmpresaDTO>> ObtenerTodos();
        Task<EmpresaDTO> ObtenerXId(int id);
        Task<EmpresaDTO> CrearYObtener(EmpresaDTO empresa);
        Task<RespuestaDTO> Editar(EmpresaDTO empresa);
        Task<RespuestaDTO> Eliminar(int id);


    }
}
