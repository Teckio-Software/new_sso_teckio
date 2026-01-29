using API_SSO.DTO;
using API_SSO.Modelos;
using API_SSO.Servicios.Contratos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios
{
    public class CatalogoClaimService<T> : ICatalogoClaimService<T> where T : DbContext
    {
        private readonly IGenericRepository<CatalogoClaim, T> _repository;
        private readonly IMapper _Mapper;

        public CatalogoClaimService(IGenericRepository<CatalogoClaim, T> repository, IMapper mapper)
        {
            _repository = repository;
            _Mapper = mapper;
        }

        public async Task<List<CatalogoClaimDTO>> ObtenerTodos()
        {
            var lista = await _repository.ObtenerTodos();
            return _Mapper.Map<List<CatalogoClaimDTO>>(lista);
        }
    }
}
