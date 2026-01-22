using API_SSO.DTO;
using API_SSO.Modelos;
using API_SSO.Servicios.Contratos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios
{
    public class RolService<T> : IRolService<T> where T : DbContext
    {
        private readonly IGenericRepository<Rol, T> _repository;
        private readonly IMapper _Mapper;

        public RolService(IGenericRepository<Rol, T> repository, IMapper mapper)
        {
            _repository = repository;
            _Mapper = mapper;
        }

        public async Task<RolDTO> CrearYObtener(RolDTO rol)
        {
            rol.FechaRegistro = DateTime.Now;
            rol.DeSistema = false;
            rol.General = false;
            rol.Activo = true;
            var objetoCreado = await _repository.Crear(_Mapper.Map<Rol>(rol));
            return _Mapper.Map<RolDTO>(objetoCreado);
        }

        public async Task<RespuestaDTO> Editar(RolDTO rol)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var objetoEncontrado = await _repository.Obtener(r => r.Id == rol.Id);
            if (objetoEncontrado.Id <= 0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "No se encontró el rol.";
                return respuesta;
            }
            objetoEncontrado.Descripcion = rol.Descripcion;
            objetoEncontrado.Color = rol.Color;
            objetoEncontrado.Activo = rol.Activo;
            objetoEncontrado.General = rol.General;
            respuesta.Estatus = await _repository.Editar(objetoEncontrado);
            respuesta.Descripcion = respuesta.Estatus ? "Rol editado correctamente." : "Ocurrió un error al intentar editar el rol.";
            return respuesta;
        }

        //public async Task<RespuestaDTO> Eliminar(int id)
        //{
        //    RespuestaDTO respuesta = new RespuestaDTO();
        //    var objetoEncontrado = await _repository.Obtener(r => r.Id == id);
        //    if (objetoEncontrado.Id <= 0)
        //    {
        //        respuesta.Estatus = false;
        //        respuesta.Descripcion = "No se encontró el rol.";
        //        return respuesta;
        //    }
        //    objetoEncontrado.Borrado = true;
        //    respuesta.Estatus = await _repository.Editar(objetoEncontrado);
        //    respuesta.Descripcion = respuesta.Estatus ? "Rol eliminado correctamente." : "Ocurrió un error al intentar eliminar el rol.";
        //    return respuesta;
        //}

        public async Task<List<RolDTO>> ObtenerTodos()
        {
            var lista = await _repository.ObtenerTodos();
            return _Mapper.Map<List<RolDTO>>(lista);
        }

        public async Task<RolDTO> ObtenerXId(int id)
        {
            var resultado = await _repository.Obtener(r => r.Id == id);
            return _Mapper.Map<RolDTO>(resultado);
        }
    }
}
