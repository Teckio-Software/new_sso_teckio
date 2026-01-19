using System.IdentityModel.Tokens.Jwt;

namespace API_SSO.Servicios.Contratos
{
    public interface ITokenInvitateService
    {
        string CrearToken(string email, string invitationId, string jti, string issuer, string audience, TimeSpan lifetime);
        (bool ok, JwtSecurityToken? token, string? error) Validate(string token, string issuer, string audience, string signingKey);
    }
}
