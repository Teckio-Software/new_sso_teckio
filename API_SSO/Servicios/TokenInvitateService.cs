using API_SSO.Servicios.Contratos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_SSO.Servicios
{
    public class TokenInvitateService : ITokenInvitateService
    {
        private readonly byte[] _key;
        public TokenInvitateService(IConfiguration cfg)
        {
            _key = Encoding.UTF8.GetBytes(cfg["llavejwt"]!);
        }

        public string CrearToken(string email, string invitationId, string jti, string issuer, string audience, TimeSpan lifetime)
        {
            var creds = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, email),
                new("invId", invitationId),
                new("onboarding", "true"),
                new(JwtRegisteredClaimNames.Jti, jti)
            };

            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(issuer: issuer, audience: audience, claims: claims, notBefore: now, expires: now.Add(lifetime), signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public (bool ok, JwtSecurityToken? token, string? error) Validate(string token, string issuer, string audience, string signingKey)
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var parms = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };

                handler.ValidateToken(token, parms, out var secToken);
                return (true, (JwtSecurityToken)secToken, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
