using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Graph.Models;
using System.Security.Claims;

namespace API_SSO.Procesos
{
    public class UsuarioEmpresasProceso
    {
        private readonly IUsuarioxEmpresaService<SSOContext> _usuarioxEmpresaService;
        private readonly IEmpresaService<SSOContext> _empresaService;
        private readonly IEmpresaXclienteService<SSOContext> _empresaXclienteService;
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly RolProceso _rolProceso;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly LogProceso _logProceso;
        private readonly IClienteService<SSOContext> _clienteService;
        public UsuarioEmpresasProceso(IUsuarioxEmpresaService<SSOContext> usuarioxEmpresaService, IEmpresaService<SSOContext> empresaService, UserManager<IdentityUser> userManager, RolProceso rolProceso, RoleManager<IdentityRole> roleManager, LogProceso logProceso, IEmpresaXclienteService<SSOContext> empresaXclienteService, IClienteService<SSOContext> clienteService)
        {
            _usuarioxEmpresaService = usuarioxEmpresaService;
            _empresaService = empresaService;
            _UserManager = userManager;
            _rolProceso = rolProceso;
            _roleManager = roleManager;
            _logProceso = logProceso;
            _empresaXclienteService = empresaXclienteService;
            _clienteService = clienteService;
        }

        public async Task<List<EmpresaDTO>> ObtenerEmpresasXUsuario(string idUsuario, List<Claim> claims)
        {
            var IdUs = claims.Find(c => c.Type == "guid")?.Value;
            if (IdUs == null)
            {
                return new List<EmpresaDTO>();
            }
            var relaciones = await _usuarioxEmpresaService.ObtenerXIdUsuario(idUsuario);
            List<EmpresaDTO> empresas = new List<EmpresaDTO>();
            foreach (var relacion in relaciones)
            {
                var empresa = await _empresaService.ObtenerXId(relacion.IdEmpresa);
                empresas.Add(empresa);
            }
            await _logProceso.CrearLog(IdUs, "Proceso", "ObtenerEmpresasXUsuario", $"Se obtuvierón {empresas.Count} registros de empresas.");
            return empresas;
        }

        public async Task<List<EmpresaDTO>> ObtenerEmpresasPerteneciente(List<Claim> claims)
        {
            var idUsuario = claims.FirstOrDefault(c => c.Type == "guid")?.Value;
            var rol = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (idUsuario == null)
            {
                return new List<EmpresaDTO>();
            }
            List<EmpresaDTO> empresas = new List<EmpresaDTO>();
            if(rol == "Administrador")
            {
                empresas = await _empresaService.ObtenerTodos();
                return empresas;
            }
            if (rol == "Cliente")
            {
                var correo = claims.FirstOrDefault(c => c.Type ==ClaimTypes.Email)?.Value;
                if(correo == null)
                {
                    await _logProceso.CrearLog(idUsuario, "Proceso", "ObtenerEmpresasPerteneciente", $"No se encontrarón empresas debido a que falta el Id del usuario");
                    return empresas;
                }
                var cliente = await _clienteService.ObtenerXCorreo(correo);
                var relacionesCliente = await _empresaXclienteService.ObtenerPorIdCliente(cliente.Id);
                foreach (var relacion in relacionesCliente)
                {
                    var empresa = await _empresaService.ObtenerXId(relacion.IdEmpresa);
                    empresas.Add(empresa);
                }
            }
            else
            {
                var relaciones = await _usuarioxEmpresaService.ObtenerXIdUsuario(idUsuario);
                foreach (var relacion in relaciones)
                {
                    var empresa = await _empresaService.ObtenerXId(relacion.IdEmpresa);
                    empresas.Add(empresa);
                }
            }
            await _logProceso.CrearLog(idUsuario, "Proceso", "ObtenerEmpresasPerteneciente", $"Se obtuvierón {empresas.Count} registros de empresas");
            return empresas;
        }

        public async Task<List<RelacionEmpresaUsuarioDTO>> ObtenerEmpresasPertenecientePorUsuario(List<Claim> claims, string IdUsuario)
        {
            var idUsuarioAdmin = claims.FirstOrDefault(c => c.Type == "guid")?.Value;
            if (idUsuarioAdmin == null)
            {
                return new List<RelacionEmpresaUsuarioDTO>();
            }
            //var relaciones = await _usuarioxEmpresaService.ObtenerXIdUsuario(idUsuarioAdmin);
            var relaciones = await ObtenerRelacionesXUsuario(idUsuarioAdmin, claims);
            var relacionesUsuario = await _usuarioxEmpresaService.ObtenerXIdUsuario(IdUsuario);
            //if (relacionesUsuario.Count <= 0)
            //{
            //    return new List<RelacionEmpresaUsuarioDTO>();
            //}
            List<RelacionEmpresaUsuarioDTO> registros = new List<RelacionEmpresaUsuarioDTO>();
            foreach (var relacion in relaciones)
            {
                var empresa = await _empresaService.ObtenerXId(relacion.IdEmpresa);
                var rel = relacionesUsuario.Find(r => r.IdEmpresa == relacion.IdEmpresa);
                var registro = new RelacionEmpresaUsuarioDTO
                {
                    IdEmpresa = empresa.Id,
                    NombreEmpresa = empresa.NombreComercial,
                    IdUsuario = relacionesUsuario.Count>0?relacionesUsuario[0].UserId:IdUsuario,
                    Activo = rel!=null?rel.Activo:false
                };
                registros.Add(registro);
            }
            await _logProceso.CrearLog(idUsuarioAdmin, "Proceso", "ObtenerEmpresasPertenecientePorUsuario", $"Se obtuvieron {registros.Count} registros de Empresa por usuario");
            return registros;
        }

        public async Task<List<UsuarioXempresaDTO>> ObtenerRelacionesXUsuario(string IdUsuario, List<Claim> claims)
        {
            var rol = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            List<UsuarioXempresaDTO> relaciones = new List<UsuarioXempresaDTO>();
            if(rol == "Administrador")
            {
                var empresas = await _empresaService.ObtenerTodos();
                foreach(var rel in empresas)
                {
                    relaciones.Add(new UsuarioXempresaDTO
                    {
                        Id = 0,
                        UserId = IdUsuario,
                        IdEmpresa = rel.Id,
                        Eliminado = false
                    });
                }
                return relaciones;
            }
            return await _usuarioxEmpresaService.ObtenerXIdUsuario(IdUsuario);
        }

        public async Task<List<UsuarioDTO>> ObtenerUsuariosXEmpresa(int idEmpresa, List<Claim> claims)
        {
            List<UsuarioDTO> usuarios = new List<UsuarioDTO>();
            var idUsuario = claims.FirstOrDefault(c => c.Type == "guid")?.Value;
            var rol = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if(rol == "Administrador")
            {
                var listaUsuarios = _UserManager.Users.ToList();
                foreach(var user in listaUsuarios)
                {
                    UsuarioDTO usuario = new UsuarioDTO
                    {
                        Id = user.Id,
                        Correo = user.Email,
                        Nombre = user.UserName,
                    };
                    usuarios.Add(usuario);
                }
                return usuarios;
            }

            //var roles = await _rolProceso.ObtenerXEmpresa(idEmpresa);
            if (idUsuario == null)
            {
                return usuarios;
            }
            var relaciones = await _usuarioxEmpresaService.ObtenerXIdEmpresa(idEmpresa);
            foreach (var relacion in relaciones)
            {
                var user = await _UserManager.FindByIdAsync(relacion.UserId);
                if (user == null)
                {
                    continue;
                }
                if (user.Id == idUsuario)
                {
                    continue;
                }
                //var aspRolesStr = await _UserManager.GetRolesAsync(user);
                //var aspRoles = await _roleManager.FindByNameAsync(aspRolesStr);
                //var rolAsignado = roles.Find(r => aspRoles.Contains(r.IdAspNetRole));
                var rolAsignado = await _rolProceso.ObtenerRolXUsuarioXEmpresa(user, idEmpresa);
                UsuarioDTO usuario = new UsuarioDTO
                {
                    Id = user.Id,
                    Correo = user.Email,
                    Nombre = user.UserName,
                    IdRol = rolAsignado.IdAspNetRole,
                    Rol = rolAsignado.Descripcion
                };
                usuarios.Add(usuario);
            }
            await _logProceso.CrearLog(idUsuario, "Proceso", "ObtenerUsuariosXEmpresa", $"Se obtuvierón {usuarios.Count} registros de usuarios por empresa.");
            return usuarios;
        }

        public async Task<RespuestaDTO> ActivarDesactivarEmpresaEnUsuario(RelacionEmpresaUsuarioDTO parametro, List<Claim> claims)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var IdUs = claims.Find(c => c.Type == "guid")?.Value;
            if(IdUs == null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "La información del usuario es inconsistente";
                return respuesta;
            }
            var relaciones= await _usuarioxEmpresaService.ObtenerXIdEmpresa(parametro.IdEmpresa);
            var relacionExistente = relaciones.Find(r => r.UserId == parametro.IdUsuario);
            //Si ya hay una relación existente para el usuario y la empresa y se pretende desactivarla, la eliminará.
            if (relacionExistente!=null)
            {
                    relacionExistente.Activo = parametro.Activo;
                    respuesta = await _usuarioxEmpresaService.Editar(relacionExistente);
                    respuesta.Descripcion = respuesta.Estatus ? "Se actualizo el estado correctamente." : "Ocurrió un error al intentar actualizar el estado";
                    return respuesta;
            }
            relacionExistente = new UsuarioXempresaDTO
            {
                UserId = parametro.IdUsuario,
                IdEmpresa = parametro.IdEmpresa,
                Activo = true
            };
            var resultadoCreacion = await _usuarioxEmpresaService.CrearYObtener(relacionExistente);
            respuesta.Estatus = resultadoCreacion.Id>0;
            respuesta.Descripcion = respuesta.Estatus ? "Empresa asignada correctamente." : "Ocurrió un error al intentar asignar la empresa.";
            if (respuesta.Estatus)
            {
                await _logProceso.CrearLog(IdUs, "Proceso", "ActivarDesactivarEmpresaEnUsuario", "Asignación realizada exitosamente.");
            }
            else
            {
                await _logProceso.CrearLog(IdUs, "Proceso", "ActivarDesactivarEmpresaEnUsuario", "Ocurrió un problema al intentar realizar la asignación.");
            }
            return respuesta;
        }
    }
}
