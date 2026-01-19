using API_SSO.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios.Contratos
{
    public interface IComprobantePagoService<TContext> where TContext : DbContext
    {
        Task<List<ComprobantePagoDTO>> ObtenerTodos();
        Task<ComprobantePagoDTO> ObtenerXId(int id);
        Task<ComprobantePagoDTO> CrearYObtener(ComprobantePagoDTO comprobantePago);
        Task<RespuestaDTO> Editar(ComprobantePagoDTO comprobantePago);
        Task<RespuestaDTO> Eliminar(int id);


    }
}
