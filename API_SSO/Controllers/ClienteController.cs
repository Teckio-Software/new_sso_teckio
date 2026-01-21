using API_SSO.DTO;
using API_SSO.DTOs;
using API_SSO.Procesos;
using Microsoft.AspNetCore.Mvc;

namespace API_SSO.Controllers
{
    [Route("api/Cliente")]
    [ApiController]
    public class ClienteController: ControllerBase
    {
        private readonly ClienteProceso _proceso;

        public ClienteController(ClienteProceso proceso)
        {
            _proceso = proceso;
        }
        
        [HttpPost("CrearUsuario")]
        public async Task<ActionResult<RespuestaDTO>> CrearUsuario(ClienteCreacionDTO cliente)
        {
            var respuesta = await _proceso.CrearUsuario(cliente);
            return respuesta;
        }

        [HttpPost("CrearCliente")]
        public async Task<ActionResult<RespuestaDTO>> CrearCliente(ClienteConComprobanteDTO cliente)
        {
            var authen = HttpContext.User;
            var resultado = await _proceso.CrearCliente(cliente, authen.Claims.ToList());
            return resultado;
        }

        [HttpGet("todos")]
        public async Task<ActionResult<List<ClienteDTO>>> ObtenerTodos()
        {
            var lista = await _proceso.ObtenerTodos();
            return lista;
        }
    }
}
