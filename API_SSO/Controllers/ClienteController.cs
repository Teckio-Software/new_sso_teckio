using API_SSO.DTO;
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
        
        [HttpPost("CrearCliente")]
        public async Task<RespuestaDTO> CrearCliente(ClienteCreacionDTO cliente)
        {
            var respuesta = await _proceso.CrearCliente(cliente);
            return respuesta;
        }
    }
}
