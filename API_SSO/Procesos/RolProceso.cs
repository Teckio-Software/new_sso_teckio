using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Identity;

namespace API_SSO.Procesos
{
    public class RolProceso
    {
        private readonly IRolService<SSOContext> _proceso;
        private readonly RoleManager<IdentityRole> _RolManager;

        public RolProceso(IRolService<SSOContext> proceso, RoleManager<IdentityRole> roleManager)
        {
            _proceso = proceso;
            _RolManager = roleManager;
        }

        public async Task<RolDTO> CrearRol(RolCreacionDTO rol, int IdEmpresa)
        {
            var nombreRol = rol.Nombre + "-" + IdEmpresa;
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
            var rolCreado = await _proceso.CrearYObtener(new RolDTO
            {
                Descripcion = rol.Descripcion,
                Color = rol.Color,
                DeSistema = false,
                Activo = true,
                General = false,
                FechaRegistro = DateTime.Now,
                IdEmpresa = IdEmpresa,
                IdAspNetRole = identityRol.Id,
            });
            return rolCreado;
        }
    }
}
