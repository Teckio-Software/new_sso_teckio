using API_SSO.DTO;
using API_SSO.Procesos;
using Microsoft.AspNetCore.Mvc;

namespace API_SSO.Controllers
{
    [ApiController]
    [Route("api/catalogoPermiso")]
    public class CatalogoPermisosController: ControllerBase
    {
        private readonly CatalogoPermisosProcess _process;

        public CatalogoPermisosController(CatalogoPermisosProcess process)
        {
            _process = process;
        }

        [HttpGet("todos")]
        public async Task<ActionResult<List<CatalogoPermisosDTO>>> Todos()
        {
            var lista = await _process.ObtenerTodos();
            return lista;
        }
    }
}
