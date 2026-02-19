using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.DTOs;
using API_SSO.Modelos;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
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
        private readonly IEmpresaXclienteService<SSOContext> _empresaxClienteService;
        private readonly RoleManager<IdentityRole> _RolManager;
        private readonly LogProceso _logProceso;

        public ClienteProceso(UserManager<IdentityUser> usuarioManager, 
            IEmpresaService<SSOContext> empresaService, 
            BaseDeDatosProceso baseDeDatosProceso, 
            RolProceso rolProceso, 
            ComprobantePagoProceso comprobantePago, 
            IClienteService<SSOContext> clienteService, 
            SSOContext dbContext, IConfiguration configuracion, 
            IEmailService email, 
            IUsuarioxEmpresaService<SSOContext> usuarioxEmpresaService, 
            IEmpresaXclienteService<SSOContext> empresaxClienteService,
            RoleManager<IdentityRole> RolManager,
            LogProceso logProceso
            )
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
            _empresaxClienteService = empresaxClienteService;
            _RolManager = RolManager;
            _logProceso = logProceso;
        }

        public async Task<RespuestaDTO> CrearUsuario(ClienteCreacionDTO clienteCreacion, List<Claim> claims, CancellationToken ct)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var idEjecutor = claims.First(c => c.Type == "guid")?.Value;
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
            var cliente = await _clienteService.ObtenerXCorreo(clienteCreacion.CorreoElectronico);
            if (cliente.Id <= 0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Aún no existe un cliente con ese correo.";
                return respuesta;
            }
            // Iniciamos la transacción
            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                //Crea el primer usuario
                var usuarioNuevoIdentity = new IdentityUser { Email = clienteCreacion.CorreoElectronico, UserName = clienteCreacion.NombreUsuario };
                var resultado = await _UsuarioManager.CreateAsync(usuarioNuevoIdentity, clienteCreacion.Contrasena);
                if (!resultado.Succeeded)
                {
                    throw new Exception("Ocurrió un error al intentar crear el usuario.");
                }
                var usuarioCreado = await _UsuarioManager.FindByEmailAsync(clienteCreacion.CorreoElectronico);
                EmpresaDTO empresa = new EmpresaDTO
                {
                    NombreComercial = clienteCreacion.NombreEmpresa,
                    Rfc = clienteCreacion.RfcEmpresa,
                    Estatus = true,
                    FechaRegistro = DateTime.Now,
                    CodigoPostal = clienteCreacion.CpEmpresa,
                    Eliminado = false,
                    //DiaPago = clienteCreacion.DiaPago
                    //De momento esta programado para que el día de pago de la empresa sea el día en el que se registró el usuario
                    DiaPago = DateTime.Now.Day,
                };
                var empresaCreada = await _EmpresaService.CrearYObtener(empresa);
                if (empresaCreada.Id <= 0)
                {
                    throw new Exception("Ocurrió un error al intentar crear la empresa.");
                }
                EmpresaXclienteDTO empresaxCliente = new EmpresaXclienteDTO
                {
                    IdCliente = cliente.Id,
                    IdEmpresa = empresaCreada.Id,
                    Eliminado = false
                };
                var resultExC = await _empresaxClienteService.CrearYObtener(empresaxCliente);
                if (resultExC.Id <= 0)
                {
                    throw new Exception("Ocurrió un error al relacionar a la empresa con el cliente.");
                }
                var ExisteRol = await _RolManager.FindByNameAsync("Cliente");
                if (ExisteRol == null)
                {
                    IdentityRole primerRol = new IdentityRole
                    {
                        Name = "Cliente"
                    };
                    await _RolManager.CreateAsync(primerRol);
                }
                await _UsuarioManager.AddToRoleAsync(usuarioCreado, "Cliente");
                //Genera el nombre de la base de datos
                string nombreBD = clienteCreacion.NombreEmpresa + string.Format("{0:D3}", empresaCreada.Id);
                //Ejecuta el proceso para crear la base de datos
                await _baseDeDatosProceso.CrearBaseDeDatos(nombreBD);
                //Verifica si la base de datos se creó exitosamente
                var bDCreada = await _baseDeDatosProceso.VerifyInstallation(nombreBD);
                if (!bDCreada)
                {
                    throw new Exception("Ocurrió un error al intentar dar de alta la empresa.");
                }
                //Crea el proyecto dentro de la nueva base de datos
                var IdProyecto = await _baseDeDatosProceso.CrearProyecto(clienteCreacion, nombreBD);
                if (IdProyecto <= 0)
                {
                    throw new Exception("Ocurrió un error al intentar crear el proyecto.");
                }
                //Crea su FSI y FSR
                await _baseDeDatosProceso.CrearFSI(IdProyecto, nombreBD);
                await _baseDeDatosProceso.CrearFSR(IdProyecto, nombreBD); 
                List<RolCreacionDTO> roles = new List<RolCreacionDTO>();
                //Crea los roles
                foreach (var rol in clienteCreacion.roles)
                {
                    rol.IdEmpresa = empresaCreada.Id;
                    var rolCreado = await _rolProceso.CrearRol(rol, claims);
                    roles.Add(new RolCreacionDTO
                    {
                        Id = rolCreado.Id,
                        Nombre = (rol.Nombre + "-" + rol.IdEmpresa)
                    });
                }
                //Crea los usuarios invitados
                foreach (var usuario in clienteCreacion.invitaciones)
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
                    //var token = await _UsuarioManager.GeneratePasswordResetTokenAsync(invitado);
                    //var hash = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                    //await InvitarOperativo(invitado, hash, ct);
                    await InvitarOperativo(invitado, ct);
                    UsuarioXempresaDTO relacion = new UsuarioXempresaDTO
                    {
                        IdEmpresa = empresaCreada.Id,
                        UserId = invitado.Id,
                        Eliminado = false,
                    };
                    await _usuarioxEmpresaService.CrearYObtener(relacion);
                }
                await transaction.CommitAsync(ct);
                await _logProceso.CrearLog(idEjecutor, "Proceso", "CrearUsuario", $"Se creó el usuario {usuarioCreado.UserName} con el rol de cliente y la empresa {empresaCreada.NombreComercial}.");
                respuesta.Estatus = true;
                respuesta.Descripcion = "Tour completo exitosamente.";
            }
            catch (Exception ex)
    {
                // Si algo falla, se deshacen los cambios en la BD principal (SSO/Identity)
                await transaction.RollbackAsync(ct);
                await _logProceso.CrearLog(idEjecutor, "Proceso", "CrearUsuario", $"Ocurrió un error al intentar crear el usuario {ex.Message}.");
                respuesta.Estatus = false;
                respuesta.Descripcion = $"Error en el proceso: {ex.Message}";
            }
            return respuesta;
        }

        public async Task<RespuestaDTO> CrearCliente(ClienteConComprobanteDTO informacion, List<Claim> claims, CancellationToken ct)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var idEjecutor = claims.First(c => c.Type == "guid")?.Value;
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
            //Valida que no haya algún otro cliente con ese correo
            var clienteExistente = await _clienteService.ObtenerXCorreo(informacion.Correo);
            if (clienteExistente.Id > 0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ya existe un cliente con ese correo.";
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
                //Al crearse un cliente solo tendrá una empresa, por lo que por defecto se cobra todo junto por ahora
                PagoXempresa = false,
            };
            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                var clienteCreado = await _clienteService.CrearYObtener(cliente);
                if (clienteCreado.Id <= 0)
                {
                    throw new Exception("Ocurrió un problema al intentar crear el cliente.");
                }
                respuesta = await _comprobantePago.SubirComprobante(informacion.Comprobante, clienteCreado.Id, claims);
                if (!respuesta.Estatus)
                {
                    throw new Exception("Ocurrió un problema al intentar subir el comprobante");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                await _logProceso.CrearLog(idEjecutor, "Proceso", "CrearCliente", $"Error al crear cliente: {ex.Message}");
                respuesta.Estatus = false;
                respuesta.Descripcion = ex.Message;
                return respuesta;
            }
            await transaction.CommitAsync(ct);
            await _logProceso.CrearLog(idEjecutor, "Proceso", "CrearCliente", $"Se creó el cliente {cliente.RazonSocial}.");
            respuesta.Estatus = true;
            respuesta.Descripcion = "Operación completada exitosamente.";
            return respuesta;
        }

        public async Task<List<ClienteDTO>> ObtenerTodos(List<Claim> claims)
        {
            var idUsuario = claims.First(c => c.Type == "guid")?.Value;
            var lista = _dbContext.Database.SqlQueryRaw<string>(""""
                SELECT C.Id
                    ,[RazonSocial]
                    ,[Correo]
                    ,ISNULL(C.CantidadEmpresas, 0) AS CantidadEmpresas
                    ,ISNULL(C.CantidadUsuariosXEmpresa, 0) AS CantidadUsuariosXempresa
                    ,ISNULL([CostoXUsuario], 0) AS CostoXusuario
                    ,ISNULL([CorreoConfirmed], 0) AS CorreoConfirmed
                    ,ISNULL(C.Eliminado, 0) AS Eliminado
                    ,C.Estatus
                    ,C.FechaRegistro
                FROM Cliente C
                for json path
                """").ToList();

            if (lista.Count <= 0)
            {
                return new List<ClienteDTO>();
            }
            string json = string.Join("", lista);
            var datos = JsonSerializer.Deserialize<List<ClienteDTO>>(json);
            await _logProceso.CrearLog(idUsuario, "Proceso", "ObtenerTodosClientes", $"Se obtuvieron {datos.Count} clientes.");
            return datos;
        }

        public async Task InvitarOperativo(IdentityUser user, CancellationToken ct)
        {
            var appUrl = _Configuracion["baseUrl"] + "on-boarding/operativo";

            var resetToken = await _UsuarioManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

            var inv = new Invitacion
            {
                Id = Guid.NewGuid(),
                Email = user.Email,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(5),
                TokenJti = encodedToken,
            };

            _dbContext.Invitacions.Add(inv);
            await _dbContext.SaveChangesAsync(ct);

            // var link = $"{appUrl}?token={Uri.EscapeDataString(encodedToken)}";
            var link = $"{appUrl}?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(encodedToken)}";

            var subject = "Bienvenido operativo";
            var html = $@"
                <h2>Hola {user.Email} 👋</h2>
                <p>Haz click aquí para crear tu cuenta:</p>
                <p>
                    <a href=""{link}"" 
                       style=""display:inline-block;
                              padding:12px 18px;
                              background:#4F46E5;
                              color:#fff;
                              text-decoration:none;
                              border-radius:8px;
                              font-weight:bold;"">
                        Registrarse
                    </a>
                </p>";

            var from = _Configuracion["Graph:FromEmail"];

            await _email.EnviarHtml(from, user.Email, subject, html, ct);

            return;
        }

        public async Task<RespuestaDTO> EditarCliente(ClienteDTO cliente, List<System.Security.Claims.Claim> claims)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var idUsuario = claims.First(c => c.Type == "guid")?.Value;
            var clienteExistente = await _clienteService.ObtenerXId(cliente.Id);
            if (clienteExistente.Id <= 0)
            {
                respuesta.Descripcion = "No se encontró el cliente.";
                respuesta.Estatus = false;
                return respuesta;
            }
            clienteExistente.RazonSocial = cliente.RazonSocial;
            clienteExistente.Correo = cliente.Correo;
            clienteExistente.CostoXusuario = cliente.CostoXusuario;
            //clienteExistente.Estatus = cliente.Estatus;
            clienteExistente.PagoXempresa = cliente.PagoXempresa;
            clienteExistente.CorreoConfirmed = cliente.CorreoConfirmed;
            respuesta = await _clienteService.Editar(cliente);
            if(respuesta.Estatus)
            {
                await _logProceso.CrearLog(idUsuario,"Proceso", "EditarCliente","Se edito el cliente correctamente.");
            }
            else
            {
                await _logProceso.CrearLog(idUsuario,"Proceso", "EditarCliente",respuesta.Descripcion);
            }
            return respuesta;
        }

        public async Task<ClienteDTO> ObtenerClienteXId(int id, List<Claim> claims)
        {
            var idUsuario = claims.First(c => c.Type == "guid")?.Value;
            var cliente = await _clienteService.ObtenerXId(id);
            if(cliente.Id <= 0)
            {
                await _logProceso.CrearLog(idUsuario, "Proceso", "ObtenerClienteXId", $"No se encontró el cliente con id {id}");
                return new ClienteDTO();
            }
            await _logProceso.CrearLog(idUsuario, "Proceso", "ObtenerClienteXId", $"Se obtuvo el cliente con id {id}");
            return cliente;
        }
    }
}
