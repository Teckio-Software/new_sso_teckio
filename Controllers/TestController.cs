using Microsoft.AspNetCore.Mvc;

namespace API_SSO.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { mensaje = "Swagger funcionando" });
    }
}
