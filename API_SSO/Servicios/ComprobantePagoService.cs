using API_SSO.DTO;
using API_SSO.Models;
using API_SSO.Servicios.Contratos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios
{
    public class ComprobantePagoService<T> : IComprobantePagoService<T> where T : DbContext
    {
        public readonly IGenericRepository<ComprobantePago, T> _repository;
        public readonly IMapper _Mapper;

        public ComprobantePagoService(IGenericRepository<ComprobantePago, T> repository, IMapper mapper)
        {
            _repository = repository;
            _Mapper = mapper;
        }

        public async Task<ComprobantePagoDTO> CrearYObtener(ComprobantePagoDTO comprobantePago)
        {
            comprobantePago.Eliminado = false;
            comprobantePago.FechaCarga = DateTime.Now;
            var objetoCreado = await _repository.Crear(_Mapper.Map<ComprobantePago>(comprobantePago));
            return _Mapper.Map<ComprobantePagoDTO>(objetoCreado);
        }

        public async Task<RespuestaDTO> Editar(ComprobantePagoDTO comprobantePago)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            try
            {
                var objetoEncontrado = await _repository.Obtener(c => c.Id == comprobantePago.Id && (bool)!c.Eliminado);
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus= false;
                    respuesta.Descripcion = "No se encontró el comprobante de pago.";
                    return respuesta;
                }
                objetoEncontrado.IdCliente = comprobantePago.IdCliente;
                objetoEncontrado.UserId = comprobantePago.UserId;
                objetoEncontrado.Ruta = comprobantePago.Ruta;
                objetoEncontrado.Estatus = comprobantePago.Estatus;
                objetoEncontrado.IdUsuarioAutorizador = comprobantePago.IdUsuarioAutorizador;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Comprobante editado exitosamente" : "Ocurrió un error al intentar editar el comprobante de pago.";
                return respuesta;
            }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar editar el comprobante de pago.";
                return respuesta;
            }
        }

        public async Task<RespuestaDTO> Eliminar(int id)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            try
            {
                var objetoEncontrado = await _repository.Obtener(c => c.Id == id && (bool)!c.Eliminado);
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "No se encontró el comprobante de pago.";
                    return respuesta;
                }
                objetoEncontrado.Eliminado = true;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Comprobante eliminado exitosamente" : "Ocurrió un error al intentar eliminar el comprobante de pago.";
                return respuesta;
            }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar eliminar el comprobante de pago.";
                return respuesta;
            }
        }

        public async Task<List<ComprobantePagoDTO>> ObtenerTodos()
        {
            var lista = await _repository.ObtenerTodos(c => (bool)!c.Eliminado);
            return _Mapper.Map<List<ComprobantePagoDTO>>(lista);
        }

        public async Task<ComprobantePagoDTO> ObtenerXId(int id)
        {
            var resultado = await _repository.Obtener(c => (bool)!c.Eliminado && c.Id == id);
            return _Mapper.Map<ComprobantePagoDTO>(resultado);
        }
    }
}
