using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.DTOs;
using API_SSO.Modelos;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public RolProceso(IRolService<SSOContext> service, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, SSOContext ssoContext)
        {
            _service = service;
            _RolManager = roleManager;
            _userManager = userManager;
            _ssoContext = ssoContext;
        }

        public async Task<RolDTO> CrearRol(RolCreacionDTO rol)
        {
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
            return rolCreado;
        }

        public async Task<RespuestaDTO> EditarRol(RolEdicionDTO objeto)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            RolDTO modelo = new RolDTO
            {
                Descripcion = objeto.rol.Descripcion,
                Color = objeto.rol.Color,
                IdEmpresa = objeto.rol.IdEmpresa,
                IdAspNetRole = objeto.rol.IdAspNetRole,
                General = objeto.rol.General,
                Activo = objeto.rol.Activo,
            };
            respuesta = await _service.Editar(modelo);
            if (!respuesta.Estatus)
            {
                return respuesta;
            }
            if (objeto.rol.IdAspNetRole == null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar editar el rol";
                return respuesta;
            }
            var identityRole = await _RolManager.FindByIdAsync(modelo.IdAspNetRole);
            if (identityRole == null)
            {
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
            respuesta.Estatus = true;
            respuesta.Descripcion = "Rol editado exitosamente.";
            return respuesta;
        }

        public async Task<List<RolDTO>> ObtenerXEmpresa(int IdEmpresa)
        {
            var lista = await _service.ObtenerTodos();
            lista = lista.Where(r => r.IdEmpresa == IdEmpresa).ToList();
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
    }
}
