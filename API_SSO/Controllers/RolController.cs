using API_SSO.Procesos;
using Microsoft.AspNetCore.Mvc;

namespace API_SSO.Controllers
{
    [Route("api/Rol")]
    [ApiController]
    public class RolController : ControllerBase
    {
        private readonly RolProceso _proceso;

        public RolController(RolProceso proceso)
        {
            _proceso = proceso;
        }
    }
}
