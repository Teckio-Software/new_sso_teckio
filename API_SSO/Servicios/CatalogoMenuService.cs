using API_SSO.DTO;
using API_SSO.Modelos;
using API_SSO.Servicios.Contratos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios
{
    public class CatalogoMenuService<T> : ICatalogoMenuService<T> where T : DbContext
    {
        private readonly IGenericRepository<CatalogoMenu, T> _repository;
        private readonly IMapper _Mapper;

        public CatalogoMenuService(IGenericRepository<CatalogoMenu, T> repository, IMapper mapper)
        {
            _repository = repository;
            _Mapper = mapper;
        }

        public async Task<List<CatalogoMenuDTO>> ObtenerTodos()
        {
            var lista = await _repository.ObtenerTodos();
            return _Mapper.Map<List<CatalogoMenuDTO>>(lista);
        }
    }
}
