using API_SSO.DTO;
using API_SSO.Models;
using API_SSO.Servicios.Contratos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios
{
    public class EmpresaService<T> : IEmpresaService<T> where T : DbContext
    {
        private readonly IGenericRepository<Empresa, T> _repository;
        private readonly IMapper _Mapper;

        public EmpresaService(IGenericRepository<Empresa, T> repository, IMapper mapper)
        {
            _repository = repository;
            _Mapper = mapper;
        }

        public async Task<EmpresaDTO> CrearYObtener(EmpresaDTO empresa)
        {
            empresa.Eliminado = false;
            empresa.FechaRegistro = DateTime.Now;
            var objetoCreado = await _repository.Crear(_Mapper.Map<Empresa>(empresa));
            return _Mapper.Map<EmpresaDTO>(objetoCreado);
        }

        public async Task<RespuestaDTO> Editar(EmpresaDTO empresa)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            try
            {
                var objetoEncontrado = await _repository.Obtener(e => e.Id == empresa.Id && (bool)!e.Eliminado);
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "No se encontró la empresa.";
                    return respuesta;
                }
                objetoEncontrado.NombreComercial = empresa.NombreComercial;
                objetoEncontrado.Rfc = empresa.Rfc;
                objetoEncontrado.Estatus = empresa.Estatus;
                objetoEncontrado.CodigoPostal = empresa.CodigoPostal;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Empresa editada exitosamente." : "Ocurrió un error al intentar editar la empresa.";
                return respuesta;
            }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar editar la empresa.";
                return respuesta;
            }
        }

        public async Task<RespuestaDTO> Eliminar(int id)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            try
            {
                var objetoEncontrado = await _repository.Obtener(e => e.Id == id && (bool)!e.Eliminado);
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "No se encontró la empresa.";
                    return respuesta;
                }
                objetoEncontrado.Eliminado = true;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Empresa eliminada exitosamente." : "Ocurrió un error al intentar eliminar la empresa.";
                return respuesta;
            }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar eliminar la empresa.";
                return respuesta;
            }
        }

        public async Task<List<EmpresaDTO>> ObtenerTodos()
        {
            var lista = await _repository.ObtenerTodos(e => (bool)!e.Eliminado);
            return _Mapper.Map<List<EmpresaDTO>>(lista);
        }

        public async Task<EmpresaDTO> ObtenerXId(int id)
        {
            var objetoEncontrado = await _repository.Obtener(e => e.Id == id && (bool)!e.Eliminado);
            return _Mapper.Map<EmpresaDTO>(objetoEncontrado);
        }
    }
}
