using API_SSO.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios.Contratos
{
    public interface ICatalogoClaimService<T> where T : DbContext
    {
        public Task<List<CatalogoClaimDTO>> ObtenerTodos();
    }
}
