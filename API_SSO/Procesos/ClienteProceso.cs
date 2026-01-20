using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.DTOs;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace API_SSO.Procesos
{
    public class ClienteProceso
    {
        private readonly UserManager<IdentityUser> _UsuarioManager;
        private readonly IEmpresaService<SSOContext> _EmpresaService;
        private readonly BaseDeDatosProceso _baseDeDatosProceso;
        private readonly RoleManager<IdentityRole> _RolManager;
        private readonly RolProceso _rolProceso;
        private readonly ComprobantePagoProceso _comprobantePago;
        private readonly IClienteService<SSOContext> _clienteService;

        public ClienteProceso(UserManager<IdentityUser> usuarioManager, IEmpresaService<SSOContext> empresaService, BaseDeDatosProceso baseDeDatosProceso, RoleManager<IdentityRole> rolManager, RolProceso rolProceso, ComprobantePagoProceso comprobantePago, IClienteService<SSOContext> clienteService)
        {
            _UsuarioManager = usuarioManager;
            _EmpresaService = empresaService;
            _baseDeDatosProceso = baseDeDatosProceso;
            _RolManager = rolManager;
            _rolProceso = rolProceso;
            _comprobantePago = comprobantePago;
            _clienteService = clienteService;
        }

        public async Task<RespuestaDTO> CrearUsuario(ClienteCreacionDTO clienteCreacion)
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
                respuesta.Descripcion = "Capture un correo electrónico válido";
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
                Eliminado = false
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
            List<RolDTO> roles = new List<RolDTO>();
            //Crea los roles
            foreach(var rol in clienteCreacion.roles)
            {
                var rolCreado = await _rolProceso.CrearRol(rol, empresaCreada.Id);
                roles.Add(rolCreado);
            }
            //Crea los usuarios invitados
            foreach(var usuario in clienteCreacion.invitaciones)
            {
                var invitado = new IdentityUser
                {
                    Email = usuario.correoInvitado,
                    UserName = usuario.nombreInvitado
                };
                await _UsuarioManager.CreateAsync(invitado);
                //Asigna el rol al usuario
                var invitadoObtenido = await _UsuarioManager.FindByEmailAsync(usuario.correoInvitado);
                if (invitadoObtenido != null)
                {
                    await _UsuarioManager.AddToRoleAsync(invitadoObtenido, roles[usuario.rolInvitado].Nombre);
                }
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
                DiaPago = informacion.DiaPago,
                CantidadEmpresas = informacion.CantidadEmpresas,
                CantidadUsuariosXempresa = informacion.CantidadUsuariosXEmpresa,
                CostoXusuario = informacion.DiaPago,
                CorreoConfirmed = false,
                Eliminado = false
            };
            var clienteCreado = _clienteService.CrearYObtener(cliente);
            if (cliente.Id <= 0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un problema al intentar crear al cliente";
                return respuesta;
            }
            respuesta = await _comprobantePago.SubirComprobante(informacion.Comprobante, clienteCreado.Id, claims);
            return respuesta;
        }
    }
}
