using API_SSO.DTO;
using API_SSO.DTOs;
using API_SSO.Models;
using API_SSO.Servicios.Contratos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios
{
    public class ClienteService<T> : IClienteService<T> where T : DbContext
    {
        private readonly IGenericRepository<Cliente, T> _repository;
        private readonly IMapper _Mapper;

        public ClienteService(IGenericRepository<Cliente, T> repository, IMapper mapper)
        {
            _repository = repository;
            _Mapper = mapper;
        }

        public async Task<ClienteDTO> CrearYObtener(ClienteDTO clienteDTO)
        {
            clienteDTO.Eliminado = false;
            clienteDTO.Estatus = true;
            clienteDTO.FechaRegistro = DateTime.Now;
            var objetoCreado = await _repository.Crear(_Mapper.Map<Cliente>(clienteDTO));
            return _Mapper.Map<ClienteDTO>(objetoCreado);
        }

        public async Task<RespuestaDTO> Editar(ClienteDTO clienteDTO)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            try
            {
                var objetoEncontrado = await _repository.Obtener(c => (c.Id == clienteDTO.Id && (bool)!c.Eliminado));
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "No se encontró el cliente.";
                    return respuesta;
                }
                objetoEncontrado.RazonSocial = clienteDTO.RazonSocial;
                objetoEncontrado.Correo = clienteDTO.Correo;
                //objetoEncontrado.DiaPago = clienteDTO.DiaPago;
                objetoEncontrado.CantidadEmpresas = clienteDTO.CantidadEmpresas;
                objetoEncontrado.CantidadUsuariosXempresa = clienteDTO.CantidadUsuariosXempresa;
                objetoEncontrado.CostoXusuario = clienteDTO.CostoXusuario;
                objetoEncontrado.CorreoConfirmed = clienteDTO.CorreoConfirmed;
                objetoEncontrado.Estatus = clienteDTO.Estatus;
                objetoEncontrado.PagoXempresa = clienteDTO.PagoXempresa;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Cliente editado exitosamente." : "Ocurrio un error al intentar editar el cliente.";
                return respuesta;
    }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrio un error al intentar editar el cliente.";
                return respuesta;
            }
        }

        public async Task<RespuestaDTO> Eliminar(int id)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            try
            {
                var objetoEncontrado = await _repository.Obtener(c => (c.Id == id && (bool)!c.Eliminado));
                if (objetoEncontrado.Id <= 0)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "No se encontró el cliente.";
                    return respuesta;
                }
                objetoEncontrado.Eliminado = true;
                respuesta.Estatus = await _repository.Editar(objetoEncontrado);
                respuesta.Descripcion = respuesta.Estatus ? "Cliente eliminado exitosamente." : "Ocurrio un error al intentar eliminar el cliente.";
                return respuesta;
            }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrio un error al intentar eliminar el cliente.";
                return respuesta;
            }
        }

        public async Task<List<ClienteDTO>> ObtenerTodos()
        {
            var lista = await _repository.ObtenerTodos(c => (bool)!c.Eliminado);
            return _Mapper.Map<List<ClienteDTO>>(lista);
        }

        public async Task<ClienteDTO> ObtenerXId(int id)
        {
            var resultado = await _repository.Obtener(c => (bool)!c.Eliminado && c.Id == id);
            return _Mapper.Map<ClienteDTO>(resultado);
        }
    }
}
