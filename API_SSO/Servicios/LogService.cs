using API_SSO.DTO;
using API_SSO.Models;
using API_SSO.Servicios.Contratos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios
{
    public class LogService<T> : ILogService<T> where T : DbContext
    {
        private readonly IGenericRepository<Log, T> _repository;
        private readonly IMapper _Mapper;

        public LogService(IGenericRepository<Log, T> repository, IMapper mapper)
        {
            _repository = repository;
            _Mapper = mapper;
        }

        public async Task<LogDTO> CrearYObtener(LogDTO log)
        {
            log.Eliminado = false;
            log.Fecha = DateTime.Now;
            var objetoCreado = await _repository.Crear(_Mapper.Map<Log>(log));
            return _Mapper.Map<LogDTO>(objetoCreado);
        }

        public async Task<RespuestaDTO> Editar(LogDTO log)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            try
            {
                var objetoEncontrado = await _repository.Obtener(l => l.Id == log.Id && (bool)!l.Eliminado);
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "no se encontró el registro.";
                    return respuesta;
                }
                objetoEncontrado.UserId = log.UserId;
                objetoEncontrado.IdEmpresa = log.IdEmpresa;
                objetoEncontrado.EsSso = log.EsSso;
                objetoEncontrado.Nivel = log.Nivel;
                objetoEncontrado.Metodo = log.Metodo;
                objetoEncontrado.Descripcion = log.Descripcion;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Registro editado exitosamente" : "Ocurrió un error al intentar editar el registro.";
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
                var objetoEncontrado = await _repository.Obtener(l => l.Id == id && (bool)!l.Eliminado);
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "no se encontró el registro.";
                    return respuesta;
                }
                objetoEncontrado.Eliminado = true;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Registro eliminado exitosamente" : "Ocurrió un error al intentar eliminar el registro.";
                return respuesta;
            }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar eliminar el registro.";
                return respuesta;
            }
        }

        public async Task<List<LogDTO>> ObtenerTodos()
        {
            var lista = await _repository.ObtenerTodos(l => (bool)!l.Eliminado);
            return _Mapper.Map<List<LogDTO>>(lista);
        }

        public async Task<List<LogDTO>> ObtenerXEmpresa(int id)
        {
            var lista = await _repository.ObtenerTodos(l => (bool)!l.Eliminado && l.IdEmpresa == id);
            return _Mapper.Map<List<LogDTO>>(lista);
        }

        public async Task<LogDTO> ObtenerXId(int id)
        {
            var objetoEncontrado = await _repository.Obtener(l => l.Id == id && (bool)!l.Eliminado);
            return _Mapper.Map<LogDTO>(objetoEncontrado);

        }
    }
}
