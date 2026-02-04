using API_SSO.DTO;
using API_SSO.Models;
using API_SSO.Servicios.Contratos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios
{
    public class ProyectoActualService<T> : IProyectoActualServce<T> where T : DbContext
    {
        private readonly IGenericRepository<ProyectoActual, T> _repository;
        private readonly IMapper _Mapper;

        public ProyectoActualService(IGenericRepository<ProyectoActual, T> repository, IMapper mapper)
        {
            _repository = repository;
            _Mapper = mapper;
        }

        public async Task<ProyectoActualDTO> CrearYObtener(ProyectoActualDTO proyectoActual)
        {
            proyectoActual.Eliminado = false;
            var objetoCreado = await _repository.Crear(_Mapper.Map<ProyectoActual>(proyectoActual));
            return _Mapper.Map<ProyectoActualDTO>(objetoCreado);
        }

        public async Task<RespuestaDTO> Editar(ProyectoActualDTO proyectoActual)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            try
            {
                var objetoEncontrado = await _repository.Obtener(p=>p.Id == proyectoActual.Id && (bool)!p.Eliminado);
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "no se encontró el registro.";
                    return respuesta;
                }
                objetoEncontrado.IdProyecto = proyectoActual.IdProyecto;
                objetoEncontrado.IdEmpresa = proyectoActual.IdEmpresa;
                objetoEncontrado.UserId = proyectoActual.UserId;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Registro editado exitosamente" : "Ocurrió un error al itentar editar el registro.";
                return respuesta;
            }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al itentar editar el registro.";
                return respuesta;
            }
        }

        public async Task<RespuestaDTO> Eliminar(int id)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            try
            {
                var objetoEncontrado = await _repository.Obtener(p => p.Id == id && (bool)!p.Eliminado);
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "no se encontró el registro.";
                    return respuesta;
                }
                objetoEncontrado.Eliminado = true;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Registro eliminado exitosamente" : "Ocurrió un error al itentar eliminar el registro.";
                return respuesta;
            }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al itentar eliminar el registro.";
                return respuesta;
            }
        }

        public async Task<List<ProyectoActualDTO>> ObtenerTodos()
        {
            var lista = await _repository.ObtenerTodos(p => (bool)!p.Eliminado);
            return _Mapper.Map<List<ProyectoActualDTO>>(lista);
        }

        public async Task<ProyectoActualDTO> ObtenerXId(int id)
        {
            var objetoEncontrado = await _repository.Obtener(p => p.Id == id && (bool)!p.Eliminado);
            return _Mapper.Map<ProyectoActualDTO>(objetoEncontrado);
        }

        public async Task<ProyectoActualDTO> ObtenerXIdUsuario(string id)
        {
            var objetoEncontrado = await _repository.Obtener(p => p.UserId == id && (bool)!p.Eliminado);
            return _Mapper.Map<ProyectoActualDTO>(objetoEncontrado);
        }
    }
}
