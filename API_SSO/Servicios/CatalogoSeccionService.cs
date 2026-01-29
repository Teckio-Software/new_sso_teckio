using API_SSO.DTO;
using API_SSO.Modelos;
using API_SSO.Models;
using API_SSO.Servicios.Contratos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios
{
    public class CatalogoSeccionService<T> : ICatalogoSeccionService<T> where T : DbContext
    {
        private readonly IGenericRepository<CatalogoSeccion, T> _repository;
        private readonly IMapper _Mapper;

        public CatalogoSeccionService(IGenericRepository<CatalogoSeccion, T> repository, IMapper mapper)
        {
            _repository = repository;
            _Mapper = mapper;
        }

        public async Task<List<CatalogoSeccionDTO>> ObtenerTodos()
        {
            var lista = await _repository.ObtenerTodos();
            return _Mapper.Map<List<CatalogoSeccionDTO>>(lista);
        }
    }
}
