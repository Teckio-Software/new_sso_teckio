using API_SSO.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios.Contratos
{
    public interface ICatalogoMenuService<T> where T : DbContext
    {
        public Task<List<CatalogoMenuDTO>> ObtenerTodos();
    }
}
