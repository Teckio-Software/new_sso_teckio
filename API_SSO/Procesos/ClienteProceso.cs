using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.DTOs;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace API_SSO.Procesos
{
    public class ClienteProceso
    {
        private readonly UserManager<IdentityUser> _UsuarioManager;
        private readonly IEmpresaService<SSOContext> _EmpresaService;
        private readonly BaseDeDatosProceso _baseDeDatosProceso;
        private readonly RolProceso _rolProceso;
        private readonly ComprobantePagoProceso _comprobantePago;
        private readonly IClienteService<SSOContext> _clienteService;
        private readonly SSOContext _dbContext;
        private readonly IConfiguration _Configuracion;
        private readonly IEmailService _email;
        private readonly IUsuarioxEmpresaService<SSOContext> _usuarioxEmpresaService;

        public ClienteProceso(UserManager<IdentityUser> usuarioManager, IEmpresaService<SSOContext> empresaService, BaseDeDatosProceso baseDeDatosProceso, RolProceso rolProceso, ComprobantePagoProceso comprobantePago, IClienteService<SSOContext> clienteService, SSOContext dbContext, IConfiguration configuracion, IEmailService email, IUsuarioxEmpresaService<SSOContext> usuarioxEmpresaService)
        {
            _UsuarioManager = usuarioManager;
            _EmpresaService = empresaService;
            _baseDeDatosProceso = baseDeDatosProceso;
            _rolProceso = rolProceso;
            _comprobantePago = comprobantePago;
            _clienteService = clienteService;
            _dbContext = dbContext;
            _Configuracion = configuracion;
            _email = email;
            _usuarioxEmpresaService = usuarioxEmpresaService;
        }

        public async Task<RespuestaDTO> CrearUsuario(ClienteCreacionDTO clienteCreacion, CancellationToken ct)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            //Primero verifica que el correo, nombre y contraseña no estén vacíos
            if (
                string.IsNullOrEmpty(clienteCreacion.CorreoElectronico)
                || string.IsNullOrEmpty(clienteCreacion.NombreUsuario)
                || string.IsNullOrEmpty(clienteCreacion.Contrasena)
                )
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Capture toda la información.";
                return respuesta;
            }
            //Valida la forma del correo electrónico
            var respuesta2 = Regex.IsMatch(clienteCreacion.CorreoElectronico,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            if (!respuesta2)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Capture un correo electrónico válido.";
                return respuesta;
            }
            //Valida que el usuario no este registrado todavía
            var ExisteUsuario = await _UsuarioManager.FindByEmailAsync(clienteCreacion.CorreoElectronico);
            if (ExisteUsuario != null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ya hay un usuario registrado con este correo.";
                return respuesta;
            }
            ExisteUsuario = await _UsuarioManager.FindByNameAsync(clienteCreacion.NombreUsuario);
            if (ExisteUsuario != null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ya hay un usuario registrado con este nombre.";
                return respuesta;
            }
            //Crea el primer usuario
            var usuarioNuevoIdentity = new IdentityUser { Email = clienteCreacion.CorreoElectronico, UserName = clienteCreacion.NombreUsuario };
            var resultado = await _UsuarioManager.CreateAsync(usuarioNuevoIdentity, clienteCreacion.Contrasena);
            if (!resultado.Succeeded)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar crear el usuario.";
                return respuesta;
            }
            //Crea el primer rol de administrador
            //var rolAdministrador = new IdentityRole
            //{
            //    Name = "Administrador"
            //};
            //var rolAdminRes = await _RolManager.CreateAsync(rolAdministrador);
            //if (rolAdminRes.Succeeded)
            //{
            var usuarioCreado = await _UsuarioManager.FindByEmailAsync(clienteCreacion.CorreoElectronico);
            await _UsuarioManager.AddToRoleAsync(usuarioCreado, "Administrador");
            //}
            //else
            //{
            //    respuesta.Estatus = false;
            //    respuesta.Descripcion = "Ocurrió un error al intentar crear el primer rol.";
            //    return respuesta;
            //}
            //Crea la empresa en el SSO
            EmpresaDTO empresa = new EmpresaDTO
            {
                NombreComercial = clienteCreacion.NombreEmpresa,
                Rfc = clienteCreacion.RfcEmpresa,
                Estatus = true,
                FechaRegistro = DateTime.Now,
                CodigoPostal = clienteCreacion.CpEmpresa,
                Eliminado = false,
                DiaPago = clienteCreacion.DiaPago
            };
            var empresaCreada = await _EmpresaService.CrearYObtener(empresa);
            if (empresaCreada.Id <= 0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar crear la empresa.";
                return respuesta;
            }
            //Genera el nombre de la base de datos
            string nombreBD = clienteCreacion.NombreEmpresa + string.Format("{0:D3}",empresaCreada.Id);
            //Ejecuta el proceso para crear la base de datos
            await _baseDeDatosProceso.CrearBaseDeDatos(nombreBD);
            //Verifica si la base de datos se creó exitosamente
            var bDCreada = await _baseDeDatosProceso.VerifyInstallation(nombreBD);
            if (!bDCreada)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar dar de alta la empresa";
                return respuesta;
            }
            //Crea el proyecto dentro de la nueva base de datos
            var IdProyecto = await _baseDeDatosProceso.CrearProyecto(clienteCreacion, nombreBD);
            if (IdProyecto <= 0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar crear el proyecto";
                return respuesta;
            }
            //Crea su FSI y FSR
            await _baseDeDatosProceso.CrearFSI(IdProyecto, nombreBD);
            await _baseDeDatosProceso.CrearFSR(IdProyecto, nombreBD);
            List<RolCreacionDTO> roles = new List<RolCreacionDTO>();
            //Crea los roles
            foreach(var rol in clienteCreacion.roles)
            {
                rol.IdEmpresa = empresaCreada.Id;
                var rolCreado = await _rolProceso.CrearRol(rol);
                roles.Add(new RolCreacionDTO
                {
                    Id = rolCreado.Id,
                    Nombre = (rol.Nombre+"-"+rol.IdEmpresa)
                });
            }
            //Crea los usuarios invitados
            foreach(var usuario in clienteCreacion.invitaciones)
            {
                //Primero comprueba si ya existe un usuario con ese correo, si existe solo le asigna el rol y lo incluye en la empresa, de lo contrario crea el usuario primero
                var existeInvitado = await _UsuarioManager.FindByEmailAsync(usuario.correoInvitado);
                var invitado = new IdentityUser
                {
                    Email = usuario.correoInvitado,
                    UserName = usuario.nombreInvitado
                };
                if (existeInvitado == null)
                {
                    var resultInvitado = await _UsuarioManager.CreateAsync(invitado);
                }
                else
                {
                    invitado = existeInvitado;
                }
                if (roles[usuario.rolInvitado] != null)
                {
                    await _UsuarioManager.AddToRoleAsync(invitado, roles[usuario.rolInvitado].Nombre);
                }
                var token = await _UsuarioManager.GeneratePasswordResetTokenAsync(invitado);
                await InvitarOperativo(invitado, token, ct);
                UsuarioXempresaDTO relacion = new UsuarioXempresaDTO
                {
                    IdEmpresa = empresaCreada.Id,
                    UserId = invitado.Id,
                    Eliminado = false,
                };
                await _usuarioxEmpresaService.CrearYObtener(relacion);
            }
            return respuesta;
        }

        public async Task<RespuestaDTO> CrearCliente(ClienteConComprobanteDTO informacion, List<System.Security.Claims.Claim> claims)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            //Primero verifica que el correo, razón social y el comprobante no estén vacíos
            if (
                string.IsNullOrEmpty(informacion.Correo)
                || string.IsNullOrEmpty(informacion.RazonSocial)
                || (informacion.Comprobante) == null
                )
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Capture toda la información.";
                return respuesta;
            }
            //Valida la forma del correo electrónico
            var respuesta2 = Regex.IsMatch(informacion.Correo,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            if (!respuesta2)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Capture un correo electrónico válido";
                return respuesta;
            }
            //Crea el cliente
            ClienteDTO cliente = new ClienteDTO
            {
                RazonSocial = informacion.RazonSocial,

                Correo = informacion.Correo,
                CantidadEmpresas = informacion.CantidadEmpresas,
                CantidadUsuariosXempresa = informacion.CantidadUsuariosXEmpresa,
                CostoXusuario = informacion.CostoXUsuario,
                CorreoConfirmed = false,
                Eliminado = false,
                Estatus = true,
                FechaRegistro = DateTime.Now,
            };
            var clienteCreado = await _clienteService.CrearYObtener(cliente);
            if (clienteCreado.Id <= 0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un problema al intentar crear al cliente";
                return respuesta;
            }
            respuesta = await _comprobantePago.SubirComprobante(informacion.Comprobante, clienteCreado.Id, claims);
            return respuesta;
        }

        public async Task<List<ClienteDTO>> ObtenerTodos()
        {
            var lista = _dbContext.Database.SqlQueryRaw<string>(""""
                SELECT C.Id
                    ,[RazonSocial]
                    ,[Correo]
                    ,(SELECT COUNT(*) FROM EmpresaXCliente EX WHERE EX.IdCliente = C.Id) AS CantidadEmpresas
                    ,(SELECT COUNT(*) FROM EmpresaXCliente EX INNER JOIN UsuarioXEmpresa UE ON EX.IdEmpresa = UE.IdEmpresa WHERE EX.IdCliente = C.Id)  AS CantidadUsuariosXEmpresa
                    ,[CostoXUsuario]
                    ,[CorreoConfirmed]
                    ,C.Eliminado
                    ,C.Estatus
                    ,C.FechaRegistro
                    ,[DiaPago]
                FROM Cliente C
                for json path
                """").ToList();

            if (lista.Count <= 0)
            {
                return new List<ClienteDTO>();
            }
            string json = string.Join("", lista);
            var datos = JsonSerializer.Deserialize<List<ClienteDTO>>(json);
            return datos;
        }

        public async Task InvitarOperativo(IdentityUser user, string hashContrasena, CancellationToken ct)
        {
            var zvClaims = new List<Claim>()
            {
                new Claim("username", user!.UserName!),
                new Claim("email", user.Email!),
                new Claim("guid", user.Id),
                new Claim("hash", hashContrasena)
            };


            var zvLlave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Configuracion["llavejwt"]!));
            var zvCreds = new SigningCredentials(zvLlave, SecurityAlgorithms.HmacSha256);
            var zvExpiracion = DateTime.UtcNow.AddHours(8);
            var zvToken = new JwtSecurityToken(issuer: null, audience: null, claims: zvClaims,
                expires: zvExpiracion, signingCredentials: zvCreds);
            var token = new JwtSecurityTokenHandler().WriteToken(zvToken);

            var appUrl = _Configuracion["baseUrl"] + "reset-password";

            var link = $"{appUrl}?token={Uri.EscapeDataString(token)}";

            var subject = "Bienvenido operativo";
            var html = $@"
                <h2>Hola {user.Email} 👋</h2>
                <p>Has da click aqúi para crear tu contraseña:</p>
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

            var from = _Configuracion["Graph:FromEmail"];

            await _email.EnviarHtml(from, user.Email, subject, html, ct);

            return;
        }
    }
}
