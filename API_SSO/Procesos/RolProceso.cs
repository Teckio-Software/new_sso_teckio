using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.DTOs;
using API_SSO.Modelos;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace API_SSO.Procesos
{
    public class RolProceso
    {
        private readonly IRolService<SSOContext> _service;
        private readonly RoleManager<IdentityRole> _RolManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SSOContext _ssoContext;
        private readonly LogProceso _logProceso;
        public RolProceso(IRolService<SSOContext> service, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, SSOContext ssoContext, LogProceso logProceso)
        {
            _service = service;
            _RolManager = roleManager;
            _userManager = userManager;
            _ssoContext = ssoContext;
            _logProceso = logProceso;
        }

        public async Task<RolDTO> CrearRol(RolCreacionDTO rol, List<Claim> claims)
        {
            //var IdUs = claims.Find(c => c.Type == "guid")?.Value;
            //if(IdUs == null)
            //{
            //    return new RolDTO();
            //}
            var nombreRol = rol.Nombre + "-" + rol.IdEmpresa;
            var nuevoRolIdentity = new IdentityRole
            {
                Name = nombreRol
            };
            await _RolManager.CreateAsync(nuevoRolIdentity);
            var identityRol = await _RolManager.FindByNameAsync(nombreRol);
            if (string.IsNullOrEmpty(identityRol.Id))
            {
                //return new RolResultDTO();
                return new RolDTO();
            }
            var rolCreado = await _service.CrearYObtener(new RolDTO
            {
                Descripcion = rol.Descripcion,
                Color = rol.Color,
                DeSistema = false,
                Activo = true,
                General = false,
                FechaRegistro = DateTime.Now,
                IdEmpresa = rol.IdEmpresa,
                IdAspNetRole = identityRol.Id,
            });
            //RolResultDTO rolResult = new RolResultDTO
            //{
            //    Id = rolCreado.Id,
            //    Descripcion = rolCreado.Descripcion,
            //    Color = rolCreado.Color,
            //    DeSistema = rolCreado.DeSistema,
            //    Activo = rolCreado.Activo,
            //    General = rolCreado.General,
            //    FechaRegistro = rolCreado.FechaRegistro,
            //    IdEmpresa = rolCreado.IdEmpresa,
            //    IdAspNetRole = rolCreado.IdAspNetRole,
            //    Nombre = nombreRol
            //};
            //Agrega los claims
            var selectedClaims = rol.Claims.Where(c => c.Selected).ToList();
            foreach (var claim in selectedClaims)
            {
                await _RolManager.AddClaimAsync(identityRol, new Claim(claim.Type, claim.Value));
            }
            //if (rolCreado.Id <= 0)
            //{
            //    await _logProceso.CrearLog(IdUs, "Proceso", "CrearRol", $"Ocurrió un error al intentar crear el rol para la empresa con Id: {rol.IdEmpresa}");
            //}
            //else
            //{
            //    await _logProceso.CrearLog(IdUs, "Proceso", "CrearRol", $"Se creó el rol con Id: {rolCreado.Id} exitosamente.");
            //}
            return rolCreado;
        }

        public async Task<RespuestaDTO> EditarRol(RolCreacionDTO objeto, List<Claim> claimsParam)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var IdUs = claimsParam.Find(c => c.Type == "guid")?.Value;
            if(IdUs == null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "No se encontró el usuario en los claims.";
                return respuesta;
            }
            RolDTO modelo = new RolDTO
            {
                Id = objeto.Id,
                Descripcion = objeto.Descripcion,
                Color = objeto.Color,
                IdEmpresa = objeto.IdEmpresa,
                IdAspNetRole = objeto.IdAspNetRole,
                General = objeto.General,
                Activo = objeto.Activo,
            };
            respuesta = await _service.Editar(modelo);
            if (!respuesta.Estatus)
            {
                await _logProceso.CrearLog(IdUs, "Proceso", "EditarRol", $"Ocurrió un error al intentar editar el rol con Id: {modelo.Id}");
                return respuesta;
            }
            if (objeto.IdAspNetRole == null)
            {

                await _logProceso.CrearLog(IdUs, "Proceso", "EditarRol", $"Ocurrió un error al intentar editar el rol con Id: {modelo.Id}, No tiene Id del rol de AspNet.");
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar editar el rol";
                return respuesta;
            }
            var identityRole = await _RolManager.FindByIdAsync(modelo.IdAspNetRole);
            if (identityRole == null)
            {

                await _logProceso.CrearLog(IdUs, "Proceso", "EditarRol", $"Ocurrió un error al intentar editar el rol con Id: {modelo.Id}, no se encontro el rol de AspNet.");
                respuesta.Estatus = false;
                respuesta.Descripcion = "No se encontró el rol";
                return respuesta;
            }
            // 1. Eliminar claims actuales
            var claims = await _RolManager.GetClaimsAsync(identityRole);
            foreach (var claim in claims)
            {
                await _RolManager.RemoveClaimAsync(identityRole, claim);
            }
            //Agrega los claims
            var selectedClaims = objeto.Claims.Where(c => c.Selected).ToList();
            foreach (var claim in selectedClaims)
            {
                await _RolManager.AddClaimAsync(identityRole, new Claim(claim.Type, claim.Value));
            }
            await _logProceso.CrearLog(IdUs, "Proceso", "EditarRol", $"Se editó el rol con Id: {modelo.Id} exitosamente.");
            respuesta.Estatus = true;
            respuesta.Descripcion = "Rol editado exitosamente.";
            return respuesta;
        }

        public async Task<List<RolDTO>> ObtenerXEmpresa(int IdEmpresa, List<Claim> claims)
        {
            var idUs = claims.Find(c => c.Type == "guid")?.Value;
            if(idUs == null)
            {
                return new List<RolDTO>();
            }
            var lista = await _service.ObtenerTodos();
            var RolAdministrador = await _RolManager.FindByNameAsync("Administrador");
            lista = lista.Where(r => r.IdEmpresa == IdEmpresa).ToList();
            if (RolAdministrador != null)
            {
                lista = lista.Where(r => r.IdAspNetRole != RolAdministrador.Id).ToList();
            }
            await _logProceso.CrearLog(idUs, "Proceso", "ObtenerXEmpresa", $"Se obtuvieron {lista.Count} registros de roles para la empresa con el Id: {IdEmpresa}");
            return lista;
        }

        public async Task<RolDTO> ObtenerRolXUsuarioXEmpresa(IdentityUser usuario, int IdEmpresa)
        {
            var sqlquery = @"
                SELECT r.Id, 
                r.FechaRegistro, 
                r.Descripcion, 
                r.Color, 
                r.IdEmpresa, 
                r.DeSistema, 
                r.General, 
                r.Activo, 
                a.Id as IdAspNetRole, 
                a.Name, 
                ar.UserId 
                FROM Rol r 
                INNER JOIN AspNetRoles a ON r.IdAspNetRole = a.Id 
                INNER JOIN AspNetUserRoles ar ON ar.RoleId = a.Id where UserId = @UserId
                for json path
                ";
            var lista = await _ssoContext.Database
                .SqlQueryRaw<string>(sqlquery, new Microsoft.Data.SqlClient.SqlParameter("@UserId", usuario.Id))
                .ToListAsync();
            if (lista.Count <= 0)
            {
                return new RolDTO();
            }
            string json = string.Join("", lista);
            var datos = JsonSerializer.Deserialize<List<RolDTO>>(json);
            if(datos == null)
            {
                return new RolDTO(); 
            }
            var rolAsignado = datos.First(r => r.IdEmpresa == IdEmpresa);
            return rolAsignado;
        }

        public async Task<List<RoleClaimViewModel>> ObtenerClaimsXRol(int idRol, List<Claim> claims)
        {
            var claimsRegistro = new List<RoleClaimViewModel>();
            var idUs = claims.First(c => c.Type == "guid")?.Value;
            if(idUs == null)
            {
                return claimsRegistro;
            }
            var rol = await _service.ObtenerXId(idRol);
            if (rol.Id <= 0)
            {
                await _logProceso.CrearLog(idUs, "Proceso", "ObtenerClaimsXRol", $"No se encontró el rol con el Id: {idRol}");
                return claimsRegistro;
            }
            var rolIdentity = await _RolManager.FindByIdAsync(rol.IdAspNetRole);
            if(rolIdentity == null)
            {
                return claimsRegistro;
            }
            var claimsObtenidos = await _RolManager.GetClaimsAsync(rolIdentity);
            foreach(var claim in claimsObtenidos)
            {
                RoleClaimViewModel claimObjeto = new RoleClaimViewModel
                {
                    Type = claim.Type,
                    Value = claim.Value,
                    Selected = true
                };
                claimsRegistro.Add(claimObjeto);
            }
            await _logProceso.CrearLog(idUs, "Proceso", "ObtenerClaimsXRol",$"Se obtuvierón {claimsRegistro} registros de claims para el rol con el Id: {idRol}");
            return claimsRegistro;
        }
    }
}
