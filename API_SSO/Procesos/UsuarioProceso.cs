using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Modelos;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Graph.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace API_SSO.Procesos
{
    public class UsuarioProceso
    {
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly RoleManager<IdentityRole> _RoleManager;
        private readonly SignInManager<IdentityUser> _SignInManager;
        private readonly IConfiguration _Configuracion;
        private readonly IEmailService _email;
        private readonly RolProceso _rolProceso;
        private readonly SSOContext _db;
        private readonly IInvitacionService _invitacionService;
        private readonly IProyectoActualServce<SSOContext> _proyectoActualServce;
        private readonly LogProceso _logProceso;
        private readonly IClienteService<SSOContext> _clienteService;
        public UsuarioProceso(UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager, 
            IConfiguration configuracion, 
            RoleManager<IdentityRole> roleManager, 
            IEmailService emailService, 
            RolProceso rolProceso, 
            SSOContext db, 
            IInvitacionService invitacionService,
            IProyectoActualServce<SSOContext> proyectoActualServce,
            LogProceso logProceso,
            IClienteService<SSOContext> clienteService
            )
        {
            _UserManager = userManager;
            _SignInManager = signInManager;
            _Configuracion = configuracion;
            _RoleManager = roleManager;
            _email = emailService;
            _rolProceso = rolProceso;
            _db = db;
            _invitacionService = invitacionService;
            _proyectoActualServce = proyectoActualServce;
            _logProceso = logProceso;
            _clienteService = clienteService;
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

        public async Task<RespuestaAutenticacionDTO> ConstruirToken(CredencialesUsuarioDTO credenciales)
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
                new Claim("username", user!.UserName),
                new Claim("email", user.Email),
                new Claim("guid", user.Id),
                new Claim("activo","1")
            };
            //var roles = await _UserManager.GetRolesAsync(user);
            //var claims = _UserManager.GetClaimsAsync(user).Result;
            List<Claim> claims = new List<Claim>();
            var usuarioUltimaSeccion = await _proyectoActualServce.ObtenerXIdUsuario(user.Id);
            var clienteEncontrado = await _clienteService.ObtenerXCorreo(user.Email);
            if (clienteEncontrado.Id > 0)
            {
                zvClaims.Add(new Claim("role", "Cliente"));
            }
            else
            {
                if (usuarioUltimaSeccion.Id > 0)
                {
                    //foreach (var item in roles)
                    //{
                    //    var rol = await _RoleManager.FindByNameAsync(item);
                    //    var registroRol = await _rolService.ObtenerXIdAsp(rol.Id);
                    //    if (registroRol.IdEmpresa != usuarioUltimaSeccion.IdEmpresa)
                    //    {
                    //        continue;
                    //    }
                    //    zvClaims.Add(new Claim("role", rol.Name));
                    //    if (rol == null)
                    //    {
                    //        continue;
                    //    }
                    //    var RolClaims = await _RoleManager.GetClaimsAsync(rol);
                    //    zvClaims.AddRange(RolClaims);
                    //}
                    var rolAsignado = await _rolProceso.ObtenerRolXUsuarioXEmpresa(user, usuarioUltimaSeccion.IdEmpresa);
                    if (rolAsignado.Id > 0)
                    {
                        var rol = await _RoleManager.FindByIdAsync(rolAsignado.IdAspNetRole);
                        zvClaims.Add(new Claim("role", rol.Name));
                        var RolClaims = await _RoleManager.GetClaimsAsync(rol);
                        zvClaims.AddRange(RolClaims);
                    }
                }
                else
                {
                    var rolesEncontrados = await _UserManager.GetRolesAsync(user);
                    if (rolesEncontrados.Count > 0)
                    {
                        zvClaims.Add(new Claim("role", rolesEncontrados[0]));
                    }
                }
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
            
            zvClaims.Add(new Claim("idUsuario", user.Id));
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

        public async Task<RespuestaDTO> AsignarRol(UsuarioRolDTO objeto, List<Claim> claims)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var IdUs = claims.First(c => c.Type == "guid")?.Value;
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
                respuesta.Descripcion = "No se encontró el rol.";
                respuesta.Estatus = false;
                return respuesta;
            }
            //if (!string.IsNullOrEmpty(objeto.AntiguoRolId))
            //{
            //    //Le quita el rol actual
            //    var rolActual = await _RoleManager.FindByNameAsync(objeto.AntiguoRolId);
            //    await _UserManager.RemoveFromRoleAsync(usuario, rolActual.Name);
            //}
            var rolActualRegistro = await _rolProceso.ObtenerRolXUsuarioXEmpresa(usuario, objeto.IdEmpresa);
            if (rolActualRegistro.Id > 0)
            {
                var rolActual = await _RoleManager.FindByIdAsync(rolActualRegistro.IdAspNetRole);
                if (rolActual != null)
                {
                    await _UserManager.RemoveFromRoleAsync(usuario, rolActual.Name);
                }
                else
                {
                    await _logProceso.CrearLog(IdUs, "Proceso", "AsignarRol", $"Intentó asignar un rol a un usuario pero no se encontró el rol actual asignado (IdUsuario: {usuario.Id}, IdEmpresa: {objeto.IdEmpresa}).");
                    respuesta.Descripcion = "No se encontró el rol.";
                    respuesta.Estatus = false;
                    return respuesta;
                }
            }

            //Agrega el rol actual
            var asignacion = await _UserManager.AddToRoleAsync(usuario, rol.Name);
            if (asignacion.Succeeded)
            {
                await _logProceso.CrearLog(IdUs, "Proceso", "AsignarRol", $"Asignó el rol {rol.Name} al usuario {usuario.UserName} ({usuario.Email}) en la empresa con Id {objeto.IdEmpresa}.");
                respuesta.Estatus = true;
                respuesta.Descripcion = "Rol asignado exitosamente.";
            }
            else
            {
                await _logProceso.CrearLog(IdUs, "Proceso", "AsignarRol", $"Ocurrió un error al intentar asignar el rol {rol.Name} al usuario {usuario.UserName} ({usuario.Email}) en la empresa con Id {objeto.IdEmpresa}.");
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un problema al intentar asignar el rol.";
            }
            return respuesta;
        }

        public async Task EnviarEmailRecuperacion(string email, CancellationToken ct)
        {
            var user = await _UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                return;
            }

            var resetToken = await _UserManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

            var inv = new Invitacion
            {
                Id = Guid.NewGuid(),
                Email = email,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(1),
                TokenJti = encodedToken,
            };

            _db.Invitacions.Add(inv);
            await _db.SaveChangesAsync(ct);

            var appUrl = _Configuracion["baseUrl"] + "reset-password";
            var link = $"{appUrl}?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(encodedToken)}";

            var subject = "Recuperación de contraseña";
            var html = $@"
                <h2>Hola {email} 👋</h2>
                <p>Haz clic aquí para restablecer tu contraseña:</p>
                <p>
                    <a href=""{link}"" 
                       style=""display:inline-block;
                              padding:12px 18px;
                              background:#4F46E5;
                              color:#fff;
                              text-decoration:none;
                              border-radius:8px;
                              font-weight:bold;"">
                        Restablecer contraseña
                    </a>
                </p>";

            var from = _Configuracion["Graph:FromEmail"];
            
            await _email.EnviarHtml(from, email, subject, html, ct);
        }

        public async Task<RespuestaDTO> RestablecerContrasena(RecuperacionContrasenaDTO objeto, CancellationToken ct)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            if (string.IsNullOrWhiteSpace(objeto.Email) || string.IsNullOrWhiteSpace(objeto.Token))
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Email y token son requeridos.";
                return respuesta;
            }

            if (string.IsNullOrWhiteSpace(objeto.NuevaContrasena))
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "La contraseña es requerida.";
                return respuesta;
            }

            var usuario = await _UserManager.FindByEmailAsync(objeto.Email);
            if (usuario == null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Usuario no encontrado";
                return respuesta;
            }

            string decodedToken;
            try
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(objeto.Token);
                decodedToken = Encoding.UTF8.GetString(tokenBytes);
            }
            catch
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Token inválido o expirado.";
                return respuesta;
            }
            var invitacion = await _invitacionService.ObtenerXToken(decodedToken);
            if (string.IsNullOrEmpty(invitacion.Id.ToString()))
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Token inválido o expirado.";
                return respuesta;
            }
            if (DateTime.Now >= invitacion.ExpiresAt || invitacion.RedeemedAt!=null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Token inválido o expirado.";
                return respuesta;
            }
            await _invitacionService.SeCompleto(invitacion.Id, ct);
            var cambio = await _UserManager.ResetPasswordAsync(usuario, decodedToken, objeto.NuevaContrasena);
            respuesta.Estatus = cambio.Succeeded;
            respuesta.Descripcion = respuesta.Estatus ? "Contraseña actualizada exitosamente." : "No fue posible cambiar la contraseña.";
            if (respuesta.Estatus)
            {
                await _logProceso.CrearLog(usuario.Id, "Proceso", "RestablecerContrasena", $"El usuario restableció su contraseña.");
            }
            else
            {
                await _logProceso.CrearLog(usuario.Id, "Proceso", "RestablecerContrasena", $"Ocurrió un error al intentar restablecer la contraseña.");
            }
            return respuesta;
        }


        public async Task<bool> ValidarToken(string email, string token)
        {
            var user = await _UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            string decodedToken;
            try
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(token);
                decodedToken = Encoding.UTF8.GetString(tokenBytes);
            }
            catch
            {
                return false;
            }

            return await _UserManager.VerifyUserTokenAsync(
                user,
                _UserManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword",
                decodedToken);
        }

        public async Task<RespuestaDTO> ValidarCorreo(string email)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            //var esValido = Regex.IsMatch(email,
            //        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            //        RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            //if (!esValido)
            //{
            //    respuesta.Estatus = false;
            //    respuesta.Descripcion = "El correo electrónico no es válido.";
            //    return false;
            //}
            var usuarioExistente = await _UserManager.FindByEmailAsync(email);
            if(usuarioExistente == null)
            {
                respuesta.Estatus = true;
                respuesta.Descripcion = "Ese correo electrónico está libre.";
                return respuesta;
            }
            respuesta.Estatus = false;
            respuesta.Descripcion = "Ya hay un usuario con ese correo.";
            return respuesta;
        }

        public async Task<RespuestaDTO> ValidarNombreUsuario(string nombre)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var usuarioExistente = await _UserManager.FindByNameAsync(nombre);
            if (usuarioExistente == null)
            {
                respuesta.Estatus = true;
                respuesta.Descripcion = "Ese nombre de usuario está libre.";
                return respuesta;
            }
            respuesta.Estatus = false;
            respuesta.Descripcion = "Ya hay un usuario con ese nombre.";
            return respuesta;
        }

        public async Task<UsuarioDTO> ObtenerUsuarioXId(string id, List<Claim> claims)
        {
            var IdUs = claims.First(c => c.Type == "guid")?.Value;
            if(IdUs == null)
            {
                return new UsuarioDTO();
            }
            var user = await _UserManager.FindByIdAsync(id);
            UsuarioDTO usuario = new UsuarioDTO
            {
                Nombre = user.UserName,
                Correo = user.Email,
                Id = user.Id
            };
            if (string.IsNullOrEmpty(usuario.Id))
            {
                await _logProceso.CrearLog(IdUs, "Proceso", "ObtenerUsuarioXId", $"Intentó obtener la información de un usuario que no existe (Id: {id}).");
            }
            else
            {
                await _logProceso.CrearLog(IdUs, "Proceso", "ObtenerUsuarioXId", $"Obtuvo la información del usuario {usuario.Nombre} ({usuario.Correo}).");
            }
            return usuario;
        }

        public async Task<RespuestaDTO> ReestableceContrasenia(CambiarContraseniaDTO parametros, List<Claim> claims)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var idEjecutor = claims.First(c => c.Type == "guid")?.Value;
            if(idEjecutor == null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "No se pudo identificar al usuario que ejecuta la acción.";
                return respuesta;
            }
            var role = claims.First(c => c.Type == ClaimTypes.Role)?.Value;
            if (role == "Administrador")
            {
                if (string.IsNullOrEmpty(parametros.IdUsuario)
                || string.IsNullOrEmpty(parametros.NuevaContrasenia)
                || string.IsNullOrEmpty(parametros.NuevaContraseniaConfirma))
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "Capture todos los campos";
                    return respuesta;
                }
                if(parametros.NuevaContrasenia != parametros.NuevaContraseniaConfirma)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "Las contraseñas no coinciden";
                    return respuesta;
                }
                var user = await _UserManager.FindByIdAsync(parametros.IdUsuario);
                if (user == null)
                {
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "No se encontró el usuario.";
                    return respuesta;
                }
                await _UserManager.RemovePasswordAsync(user);
                var resultado = await _UserManager.AddPasswordAsync(user, parametros.NuevaContrasenia);
                if (resultado.Succeeded)
                {
                    await _logProceso.CrearLog(idEjecutor, "Proceso", "ReestableceContrasenia", $"Reestableció la contraseña del usuario {user.UserName} ({user.Email})");
                    respuesta.Estatus = true;
                    respuesta.Descripcion = "Contraseña reestablecida exitosamente.";
                }
                else
                {
                    await _logProceso.CrearLog(idEjecutor, "Proceso", "ReestableceContrasenia", $"Ocurrió un error al intentar reestablecer la contraseña del usuario {user.UserName} ({user.Email})");
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "Ocurrió un error al intentar reestablecer la contraseña.";
                }
                return respuesta;
            }
            await _logProceso.CrearLog(idEjecutor, "Proceso", "ReestableceContrasenia", $"Intentó reestablecer la contraseña de un usuario sin tener permisos para ello.");
            respuesta.Estatus = false;
            respuesta.Descripcion = "El usuairo no cuenta con permisos para realizar esta acción.";
            return respuesta;
        }
    }
}
