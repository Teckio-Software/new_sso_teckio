using API_SSO.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_SSO.Controllers
{

    [Route("api/cuenta")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly SignInManager<IdentityUser> _SignInManager;
        private readonly IConfiguration _Configuracion;

        public UsuarioController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuracion)
        {
            _UserManager = userManager;
            _SignInManager = signInManager;
            _Configuracion = configuracion;
        }

        [HttpPost("login")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login([FromBody] CredencialesUsuarioDTO credenciales)
        {
            // Buscar usuario
            var user = await _UserManager.FindByEmailAsync(credenciales.Email);
            if (user == null)
            {
                //Si no coincide con el email buscará por el nombre de usuario
                user = await _UserManager.FindByNameAsync(credenciales.Email);
                if(user == null)
                {
                    return new RespuestaAutenticacionDTO
                    {
                        FechaExpiracion = DateTime.Today,
                        Token = "El usuario ingresado es incorrecto."
                    };
                }
            }

            // Validar contraseña SIN emitir cookie
            var pwdCheck = await _SignInManager.CheckPasswordSignInAsync(
                user,
                credenciales.Password,
                lockoutOnFailure: false
            );

            if (!pwdCheck.Succeeded)
            {
                return new RespuestaAutenticacionDTO
                {
                    FechaExpiracion = DateTime.Today,
                    Token = "La contraseña ingresada es incorrecta."
                };
            }

            // Emitir SOLO el JWT
            return await ConstruirToken(credenciales);
        }

        private async Task<RespuestaAutenticacionDTO> ConstruirToken(CredencialesUsuarioDTO credenciales)
        {
            var user = await _UserManager.FindByEmailAsync(credenciales.Email);
            if(user == null)
            {
                user = await _UserManager.FindByNameAsync(credenciales.Email);
            }
            var zvClaims = new List<Claim>()
            {
                new Claim("username", user!.UserName!),
                new Claim("email", user.Email!),
                new Claim("guid", user.Id),
                new Claim("activo","1")
            };
            var claims = _UserManager.GetClaimsAsync(user).Result;
            //Si tiene privilegios de Arministrador o Administrador de roles los agrega como Claim
            var claimAdministrador = claims.FirstOrDefault(z => z.Value == "Administrador");
            if (claimAdministrador != null)
            {
                zvClaims.Add(claimAdministrador);
            }
            var claimAdministradorRoles = claims.FirstOrDefault(z => z.Value == "AdministradorRoles");
            if (claimAdministradorRoles != null)
            {
                zvClaims.Add(claimAdministradorRoles);
            }
            var zvLlave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Configuracion["llavejwt"]!));
            var zvCreds = new SigningCredentials(zvLlave, SecurityAlgorithms.HmacSha256);
            var zvExpiracion = DateTime.UtcNow.AddHours(5);
            var zvToken = new JwtSecurityToken(issuer: null, audience: null, claims: zvClaims,
                expires: zvExpiracion, signingCredentials: zvCreds);
            return new RespuestaAutenticacionDTO()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(zvToken),
                FechaExpiracion = zvExpiracion
            };
        }
    }
}
