using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_SSO.Controllers
{
    [ApiController]
    [Route("api/invitacion")]
    public class InvitacionController : ControllerBase
    {
        private readonly IInvitacionService _invitacionService;
        public InvitacionController(IInvitacionService svc)
        {
            _invitacionService = svc;
        }

        public record CrearInvitacionRequest(string Email);

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearInvitacionRequest crearInvitacionRequest, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(crearInvitacionRequest.Email))
            {
                return BadRequest(new { ok = false, message = "Email requerido" });
            }

            var id = _invitacionService.CrearYEnviar(crearInvitacionRequest.Email, ct);
            return Ok(new { ok = true, invitationId = id });
        }

        public record InvitarUsuarioRequest(string Email);

        [HttpPost("usuario")]
        public async Task<IActionResult> InvitarUsuario([FromBody] InvitarUsuarioRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Email))
            {
                return BadRequest(new { ok = false, message = "Email requerido" });
            }

            var id = await _invitacionService.InvitarUsuario(req.Email, ct);
            return Ok(new { ok = true, invitationId = id });
        }

        public record RedeemRequest(string Token);

        [HttpPost]
        [Route("redeem")]
        public async Task<IActionResult> Redeem([FromBody] RedeemRequest req, CancellationToken ct)
        {
            var (ok, inv, error) = await _invitacionService.RedeemAsync(req.Token, ct);
            if (!ok || inv is null) return BadRequest(new { ok = false, message = error });

            return Ok(new
            {
                ok = true,
                invitationId = inv.Id,
                email = inv.Email,
                expiresAt = inv.ExpiresAt,
                onboarding = true
            });
        }

        public record CompleteRequest(Guid InvitationId);
        [HttpPost("complete")]
        [AllowAnonymous]
        public async Task<IActionResult> Complete([FromBody] CompleteRequest req, CancellationToken ct)
        {
            await _invitacionService.SeCompleto(req.InvitationId, ct);
            return Ok(new { ok = true });
        }
    }
}
