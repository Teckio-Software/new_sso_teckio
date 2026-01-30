using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios.Contratos;
using Microsoft.Graph.Models;
using System.Collections.Generic;

namespace API_SSO.Procesos
{
    public class CatalogoPermisosProcess
    {
        private readonly ICatalogoClaimService<SSOContext> _claimService;
        private readonly ICatalogoSeccionService<SSOContext> _seccionService;
        private readonly ICatalogoMenuService<SSOContext> _menuService;

        public CatalogoPermisosProcess(ICatalogoClaimService<SSOContext> claimService, ICatalogoSeccionService<SSOContext> seccionService, ICatalogoMenuService<SSOContext> menuService)
        {
            _claimService = claimService;
            _seccionService = seccionService;
            _menuService = menuService;
        }

        public async Task<List<CatalogoPermisoMenuDTO>> ObtenerTodos()
        {
            List<CatalogoPermisoMenuDTO> lista = new List<CatalogoPermisoMenuDTO>();
            var menus = await _menuService.ObtenerTodos();
            var secciones = await _seccionService.ObtenerTodos();
            var claims = await _claimService.ObtenerTodos();
            foreach( var menu in menus)
            {
                var sublista = new List<CatalogoPermisosDTO>();
                var seccionesFiltradas = secciones.Where(s => s.IdMenu == menu.Id).ToList();
                foreach (var seccion in seccionesFiltradas)
                {
                    var claimsPorSeccion = claims.Where(c => c.IdSeccion == seccion.Id).ToList();
                    sublista.Add(new CatalogoPermisosDTO
                    {
                        Descripcion = seccion.Descripcion,
                        Id = seccion.Id,
                        IdMenu = seccion.IdMenu,
                        Nombre = seccion.Nombre,
                        Claims = claimsPorSeccion
                    });
                }
                lista.Add(new CatalogoPermisoMenuDTO
                {
                    Id = menu.Id,
                    Nombre = menu.Nombre,
                    Secciones = sublista
                });
            }
            return lista;
        }
    }
}
