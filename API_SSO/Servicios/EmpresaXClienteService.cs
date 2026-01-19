using API_SSO.DTO;
using API_SSO.Models;
using API_SSO.Servicios.Contratos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios
{
    public class EmpresaXClienteService<T> : IEmpresaXclienteService<T> where T : DbContext
    {
        private readonly IGenericRepository<EmpresaXcliente, T> _repository;
        private readonly IMapper _Mapper;

        public EmpresaXClienteService(IGenericRepository<EmpresaXcliente, T> repository, IMapper mapper)
        {
            _repository = repository;
            _Mapper = mapper;
        }

        public async Task<EmpresaXclienteDTO> CrearYObtener(EmpresaXclienteDTO empresaXcliente)
        {
            empresaXcliente.Eliminado = false;
            var objetoCreado = await _repository.Crear(_Mapper.Map<EmpresaXcliente>(empresaXcliente));
            return _Mapper.Map<EmpresaXclienteDTO>(objetoCreado);
        }

        public async Task<RespuestaDTO> Editar(EmpresaXclienteDTO empresaXcliente)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            try
            {
                var objetoEncontrado = await _repository.Obtener(e=>e.Id == empresaXcliente.Id && (bool)!e.Eliminado);
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "No se encontró el registro.";
                    return respuesta;
                }
                objetoEncontrado.IdEmpresa = empresaXcliente.IdEmpresa;
                objetoEncontrado.IdCliente = empresaXcliente.IdCliente;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Registro editado exitosamente." : "Ocurrió un error al intentar editar el registro.";
                return respuesta;
            }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar editar el registro.";
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
                    respuesta.Descripcion = "No se encontró el registro.";
                    return respuesta;
                }
                objetoEncontrado.Eliminado = true;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Registro eliminado exitosamente." : "Ocurrió un error al intentar eliminar el registro.";
                return respuesta;
            }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar eliminar el registro.";
                return respuesta;
            }
        }

        public async Task<List<EmpresaXclienteDTO>> ObtenerTodos()
        {
            var lista = await _repository.ObtenerTodos(e=>(bool)!e.Eliminado);
            return _Mapper.Map<List<EmpresaXclienteDTO>>(lista);
        }

        public async Task<EmpresaXclienteDTO> ObtenerXId(int id)
        {
            var objetoEncontrado = await _repository.Obtener(e => e.Id == id && (bool)!e.Eliminado);
            return _Mapper.Map<EmpresaXclienteDTO>(objetoEncontrado);
        }
    }
}
