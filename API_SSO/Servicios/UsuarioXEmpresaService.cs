using API_SSO.DTO;
using API_SSO.Models;
using API_SSO.Servicios.Contratos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace API_SSO.Servicios
{
    public class UsuarioXEmpresaService<T> : IUsuarioxEmpresaService<T> where T : DbContext
    {
        private readonly IGenericRepository<UsuarioXempresa, T> _repository;
        private readonly IMapper _Mapper;

        public UsuarioXEmpresaService(IGenericRepository<UsuarioXempresa, T> repository, IMapper mapper)
        {
            _repository = repository;
            _Mapper = mapper;
        }

        public async Task<UsuarioXempresaDTO> CrearYObtener(UsuarioXempresaDTO usuarioXempresa)
        {
            usuarioXempresa.Eliminado = false;
            usuarioXempresa.Activo = true;
            var objetoCreado = await _repository.Crear(_Mapper.Map<UsuarioXempresa>(usuarioXempresa));
            return _Mapper.Map<UsuarioXempresaDTO>(objetoCreado);
        }

        public async Task<RespuestaDTO> Editar(UsuarioXempresaDTO usuarioXempresa)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            try
            {
                var objetoEncontrado = await _repository.Obtener(u => u.Id == usuarioXempresa.Id && (bool)!u.Eliminado);
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "No se encontró el registro.";
                    return respuesta;
                }
                objetoEncontrado.UserId = usuarioXempresa.UserId;
                objetoEncontrado.IdEmpresa = usuarioXempresa.IdEmpresa;
                objetoEncontrado.Activo = usuarioXempresa.Activo;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus?"Registro editado exitsamente.": "Ocurrió un error al intentar editar el registro.";
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
                var objetoEncontrado = await _repository.Obtener(u => u.Id == id && (bool)!u.Eliminado);
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "No se encontró el registro.";
                    return respuesta;
                }
                objetoEncontrado.Eliminado = true;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Registro eliminado exitsamente." : "Ocurrió un error al intentar eliminar el registro.";
                return respuesta;
            }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar eliminar el registro.";
                return respuesta;
            }
        }

        public async Task<List<UsuarioXempresaDTO>> ObtenerTodos()
        {
            var lista = await _repository.ObtenerTodos(u=>(bool)!u.Eliminado);
            return _Mapper.Map<List<UsuarioXempresaDTO>>(lista);
        }

        public async Task<UsuarioXempresaDTO> ObtenerXId(int id)
        {
            var objetoEncontrado = await _repository.Obtener(u => u.Id == id && (bool)!u.Eliminado);
            return _Mapper.Map<UsuarioXempresaDTO>(objetoEncontrado);
        }

        public async Task<List<UsuarioXempresaDTO>> ObtenerXIdEmpresa(int IdEmpresa)
        {
            var lista = await _repository.ObtenerTodos(u => (bool)!u.Eliminado && u.IdEmpresa == IdEmpresa);
            return _Mapper.Map<List<UsuarioXempresaDTO>>(lista);
        }

        public async Task<List<UsuarioXempresaDTO>> ObtenerXIdUsuario(string IdUsuario)
        {
            var lista = await _repository.ObtenerTodos(u => (bool)!u.Eliminado && u.UserId == IdUsuario);
            return _Mapper.Map<List<UsuarioXempresaDTO>>(lista);
        }
    }
}
