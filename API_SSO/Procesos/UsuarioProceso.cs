using API_SSO.DTO;
using API_SSO.Modelos;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Graph.Models;
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
        private readonly IConfiguration _cfg;
        private readonly IEmailService _email;

        public UsuarioProceso(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuracion, RoleManager<IdentityRole> roleManager, IConfiguration cfg, IEmailService emailService)
        {
            _UserManager = userManager;
            _SignInManager = signInManager;
            _Configuracion = configuracion;
            _RoleManager = roleManager;
            _cfg = cfg;
            _email = emailService;
        }

        public async Task<IdentityUser> ObtenerUsuario(string parametro)
        {
            var user = await _UserManager.FindByEmailAsync(parametro);
            if (user == null)
            {
                user = await _UserManager.FindByNameAsync(parametro);
            }
            if(user == null)
            {
                return new IdentityUser();
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
                zvClaims.Add(new Claim("rol", rol.Name));
                if (rol == null)
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

        public async Task<RespuestaDTO> AsignarRol(UsuarioRolDTO objeto)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var usuario = await _UserManager.FindByIdAsync(objeto.IdUsuario);
            if (string.IsNullOrEmpty(usuario.Id))
            {
                respuesta.Descripcion = "No se encontró el usuario.";
                respuesta.Estatus = false;
                return respuesta;
            }
            var rol = await _RoleManager.FindByIdAsync(objeto.IdRol);
            if (string.IsNullOrEmpty(rol.Id))
            {
                respuesta.Descripcion = "No se encontró el usuario.";
                respuesta.Estatus = false;
                return respuesta;
            }
            //Le quita el rol actual
            var rolActual = await _UserManager.GetRolesAsync(usuario);
            await _UserManager.RemoveFromRolesAsync(usuario, rolActual);
            //Agrega el rol actual
            var asignacion = await _UserManager.AddToRoleAsync(usuario, rol.Name);
            if (asignacion.Succeeded)
            {
                respuesta.Estatus = true;
                respuesta.Descripcion = "Rol asignado exitosamente.";
            }
            else
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un problema al intentar asignar el rol.";
            }
            return respuesta;
        }

        public async Task EnviarEmailRecuperacion(string email, CancellationToken ct)
        {
            //Crea el token
            //var user = await ObtenerUsuario(email);
            var user = await _UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                return;
            }
            var hashContrasena = await _UserManager.GeneratePasswordResetTokenAsync(user);
            var zvClaims = new List<Claim>()
            {
                new Claim("username", user!.UserName!),
                new Claim("email", user.Email!),
                new Claim("guid", user.Id),
                new Claim("hash", hashContrasena)
            };

            
            var zvLlave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Configuracion["llavejwt"]!));
            var zvCreds = new SigningCredentials(zvLlave, SecurityAlgorithms.HmacSha256);
            var zvExpiracion = DateTime.UtcNow.AddHours(1);
            var zvToken = new JwtSecurityToken(issuer: null, audience: null, claims: zvClaims,
                expires: zvExpiracion, signingCredentials: zvCreds);
            var token = new JwtSecurityTokenHandler().WriteToken(zvToken);

            var appUrl = _cfg["baseUrl"] + "reset-pasword";

            var link = $"{appUrl}?token={Uri.EscapeDataString(token)}";

            var subject = "Recuperación de contraseña";
            var html = $@"
                <h2>Hola {email} 👋</h2>
                <p>Has da click aqúi para restablecer tu contraseña:</p>
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

            return;

        }

        public async Task<RespuestaDTO> RestablecerContrasena(RecuperacionContrasenaDTO objeto, List<System.Security.Claims.Claim> claims, string JWT)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            if (string.IsNullOrWhiteSpace(JWT) || !JWT.StartsWith("Bearer "))
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Token no encontrado o formato inválido.";
                return respuesta;
            }

            // Extraer el token como string
            var tokenString = JWT.Substring("Bearer ".Length).Trim();

            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(tokenString))
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Formato de token inválido.";
                return respuesta;
            }

            var token = handler.ReadJwtToken(tokenString);

            // 'exp' es la fecha de expiración en segundos desde 1970-01-01 UTC
            var expClaim = token.Payload.Exp;

            if (expClaim == null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "El token no contiene fecha de expiración.";
            }

            // Convertir a DateTime UTC
            DateTime fechaExp = DateTimeOffset.FromUnixTimeSeconds(expClaim.Value).UtcDateTime;

            if( DateTime.UtcNow >= fechaExp)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "El token ha expirado.";
                return respuesta;
            }

            var email = claims.Where(z => z.Type == "email").ToList();
            if (email.Count <= 0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "La información del token es incorrecta.";
                return respuesta;
            }
            objeto.Email = email[0].Value;
            var usuario = await _UserManager.FindByEmailAsync(objeto.Email);
            var hashContrasena = claims.Where(z => z.Type == "hash").ToList();
            if (usuario == null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "No se encontró el usuario.";
                return respuesta;
            }
            if (hashContrasena.Count<=0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "La información del token es incorrecta.";
                return respuesta;
            }
            var cambio = await _UserManager.ResetPasswordAsync(usuario, hashContrasena[0].Value, objeto.NuevaContrasena);
            respuesta.Estatus = cambio.Succeeded;
            respuesta.Descripcion = respuesta.Estatus ? "Contraseña actualizada exitosamente." : "No fue posible cambiar la contraseña.";
            return respuesta;
        }
    }
}
