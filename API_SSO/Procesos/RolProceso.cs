using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace API_SSO.Procesos
{
    public class RolProceso
    {
        private readonly IRolService<SSOContext> _service;
        private readonly RoleManager<IdentityRole> _RolManager;

        public RolProceso(IRolService<SSOContext> service, RoleManager<IdentityRole> roleManager)
        {
            _service = service;
            _RolManager = roleManager;
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
            //Agrega los claims
            var selectedClaims = rol.Claims.Where(c => c.Selected).ToList();
            foreach (var claim in selectedClaims)
            {
                await _RolManager.AddClaimAsync(identityRol, new Claim("Permission", claim.Value));
            }
            return rolCreado;
        }

        //public async Task<RespuestaDTO> EditarRol(RolEdicionDTO objeto)
        //{
        //    RespuestaDTO respuesta = new RespuestaDTO();
        //    RolDTO modelo = new RolDTO
        //    {
        //        Descripcion = objeto.rol.Descripcion,
        //        Color = objeto.rol.Color,
        //        IdEmpresa = objeto.rol.IdEmpresa,
        //        IdAspNetRole = objeto.rol.IdAspNetRole,
        //        General = objeto.rol.General,
        //        Activo = objeto.rol.Activo,
        //    };
        //    respuesta = await _service.Editar(modelo);
        //    if (!respuesta.Estatus)
        //    {
        //        return respuesta;
        //    }
        //    if(objeto.rol.IdAspNetRole == null)
        //    {
        //        respuesta.Estatus = false;
        //        respuesta.Descripcion = "Ocurrió un error al intentar editar el rol";
        //        return respuesta;
        //    }
        //    var identityRole = await _RolManager.FindByIdAsync(modelo.IdAspNetRole);
        //    if (identityRole == null)
        //    {
        //        respuesta.Estatus = false;
        //        respuesta.Descripcion = "No se encontró el rol";
        //        return respuesta;
        //    }
        //    await _RolManager.RemoveClaimAsync(identityRole);
        //}
    }
}
