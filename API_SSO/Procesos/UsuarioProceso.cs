using API_SSO.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_SSO.Procesos
{
    public class UsuarioProceso
    {
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly RoleManager<IdentityRole> _RoleManager;
        private readonly SignInManager<IdentityUser> _SignInManager;
        private readonly IConfiguration _Configuracion;

        public UsuarioProceso(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuracion, RoleManager<IdentityRole> roleManager)
        {
            _UserManager = userManager;
            _SignInManager = signInManager;
            _Configuracion = configuracion;
            _RoleManager = roleManager;
        }

        public async Task<IdentityUser> ObtenerUsuario(string parametro)
        {
            var user = await _UserManager.FindByEmailAsync(parametro);
            if (user == null)
            {
                user = await _UserManager.FindByNameAsync(parametro);
            }
            return user;
        }

        public async Task<RespuestaAutenticacionDTO> Login(CredencialesUsuarioDTO credenciales)
        {
            // Buscar usuario
            var user = await ObtenerUsuario(credenciales.Email);
            if (user == null)
            {
               return new RespuestaAutenticacionDTO
               {
                  FechaExpiracion = DateTime.Today,
                  Token = "El usuario ingresado es incorrecto."
               };
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
            var user = await ObtenerUsuario(credenciales.Email);
            if (user == null)
            {
                return new RespuestaAutenticacionDTO
                {
                    FechaExpiracion = DateTime.Today,
                    Token = "El usuario ingresado es incorrecto."
                };
            }
            var zvClaims = new List<Claim>()
            {
                new Claim("username", user!.UserName!),
                new Claim("email", user.Email!),
                new Claim("guid", user.Id),
                new Claim("activo","1")
            };
            var roles = await _UserManager.GetRolesAsync(user);
            //var claims = _UserManager.GetClaimsAsync(user).Result;
            List<Claim> claims = new List<Claim>();
            foreach (var item in roles)
            {
                var rol = await _RoleManager.FindByNameAsync(item);
                if(rol == null)
                {
                    continue;
                }
                var RolClaims = await _RoleManager.GetClaimsAsync(rol);
                claims.AddRange(RolClaims);
            }
            //Si tiene privilegios de Super usuario o de Panel administrador los agrega como Claim
            var claimAdministrador = claims.FirstOrDefault(z => z.Value == "Super usuario");
            if (claimAdministrador != null)
            {
                zvClaims.Add(claimAdministrador);
            }
            var claimAdministradorRoles = claims.FirstOrDefault(z => z.Value == "Panel administrador");
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
