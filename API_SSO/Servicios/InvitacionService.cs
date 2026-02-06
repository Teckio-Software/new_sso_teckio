using API_SSO.Context;
using API_SSO.Modelos;
using API_SSO.Servicios.Contratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API_SSO.Servicios
{
    public class InvitacionService : IInvitacionService
    {
        private readonly SSOContext _db;
        private readonly ITokenInvitateService _tokenSvc;
        private readonly IEmailService _email;
        private readonly IConfiguration _cfg;

        public InvitacionService(SSOContext db, ITokenInvitateService tokenSvc, IEmailService email, IConfiguration cfg)
        {
            _db = db;
            _tokenSvc = tokenSvc;
            _email = email;
            _cfg = cfg;
        }

        public async Task<Guid> CrearYEnviar(string email, CancellationToken ct)
        {
            var inv = new Invitacion
            {
                Id = Guid.NewGuid(),
                Email = email,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(5),
                TokenJti = Guid.NewGuid().ToString("N")
            };

            _db.Invitacions.Add(inv);
            await _db.SaveChangesAsync(ct);

            var issuer = "teckioerp";
            var audience = "onboarding";

            var token = _tokenSvc.CrearToken(
                    email: inv.Email,
                    invitationId: inv.Id.ToString(),
                    jti: inv.TokenJti,
                    issuer: issuer,
                    audience: audience,
                    lifetime: TimeSpan.FromDays(5)
                );



            var appUrl = _cfg["baseUrl"] + "on-boarding";

            var link = $"{appUrl}?token={Uri.EscapeDataString(token)}";

            var subject = "¡Bienvenido a Teckio Software!";
            var html = $@"
                <h2>Hola {email} 👋</h2>
                <p>Has sido invitado a conocer el sistema. Da clic para iniciar el tour:</p>
                <p>
                    <a href=""{link}"" 
                       style=""display:inline-block;
                              padding:12px 18px;
                              background:#4F46E5;
                              color:#fff;
                              text-decoration:none;
                              border-radius:8px;
                              font-weight:bold;"">
                        Comenzar tour 🚀
                    </a>
                </p>";

            var from = _cfg["Graph:FromEmail"];

            await _email.EnviarHtml(from, email, subject, html, ct);

            return inv.Id;

        }

        public async Task<Guid> InvitarUsuario(string email, CancellationToken ct)
        {
            var inv = new Invitacion
            {
                Id = Guid.NewGuid(),
                Email = email,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                TokenJti = Guid.NewGuid().ToString("N")
            };

            _db.Invitacions.Add(inv);
            await _db.SaveChangesAsync(ct);

            var issuer = "teckioerp";
            var audience = "user-invitation";

            var token = _tokenSvc.CrearToken(
                    email: inv.Email,
                    invitationId: inv.Id.ToString(),
                    jti: inv.TokenJti,
                    issuer: issuer,
                    audience: audience,
                    lifetime: TimeSpan.FromDays(7)
                );

            var appUrl = _cfg["baseUrl"] + "register";

            var link = $"{appUrl}?token={Uri.EscapeDataString(token)}";

            var subject = "¡Invitación a unirse a Teckio Software!";
            var html = $@"
                <h2>Hola {email}</h2>
                <p>Has sido invitado a unirte a nuestro sistema. Da clic para registrarte:</p>
                <p>
                    <a href=""{link}"" 
                       style=""display:inline-block;
                              padding:12px 18px;
                              background:#4F46E5;
                              color:#fff;
                              text-decoration:none;
                              border-radius:8px;
                              font-weight:bold;"">
                        Aceptar invitación 🎉
                    </a>
                </p>
                <p>Este enlace expira en 7 días.</p>";

            var from = _cfg["Graph:FromEmail"];

            await _email.EnviarHtml(from, email, subject, html, ct);

            return inv.Id;
        }

        public async Task<(bool ok, Invitacion? invitacion, string? error)> RedeemAsync(string token, CancellationToken ct)
        {
            var issuer = "teckioerp";
            var audience = "onboarding";
            var signingKey = _cfg["llavejwt"]!;

            var (ok, jwt, err) = _tokenSvc.Validate(token, issuer, audience, signingKey);
            if (!ok || jwt is null) return (false, null, err ?? "token inválido");

            var invIdStr = jwt.Claims.FirstOrDefault(c => c.Type == "invId")?.Value;
            var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (!Guid.TryParse(invIdStr, out var invId)) return (false, null, "invId inválido");

            var inv = await _db.Invitacions.FirstOrDefaultAsync(i => i.Id == invId, ct);
            if (inv is null) return (false, null, "invitación no encontrada");
            if (inv.RedeemedAt != null) return (false, null, "invitación ya usada");
            if (inv.ExpiresAt < DateTimeOffset.UtcNow) return (false, null, "invitación expirada");
            if (!string.Equals(inv.TokenJti, jti, StringComparison.Ordinal))
                return (false, null, "token no coincide");

            return (true, inv, null); // 34
        }

        public async Task SeCompleto(Guid invitationId, CancellationToken ct)
        {
            var inv = await _db.Invitacions.FirstOrDefaultAsync(i => i.Id == invitationId, ct);
            if (inv is null) return;
            inv.RedeemedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<Invitacion> ObtenerXToken(string token)
        {
            var invitacion = await _db.Invitacions.Where(i=>i.TokenJti == token).FirstOrDefaultAsync();
            if (invitacion==null)
            {
                return new Invitacion();
            }
            return invitacion;
        }
    }
}
