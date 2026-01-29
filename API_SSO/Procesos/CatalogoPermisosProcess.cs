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

        public CatalogoPermisosProcess(ICatalogoClaimService<SSOContext> claimService, ICatalogoSeccionService<SSOContext> seccionService)
        {
            _claimService = claimService;
            _seccionService = seccionService;
        }

        public async Task<List<CatalogoPermisosDTO>> ObtenerTodos()
        {
            List<CatalogoPermisosDTO> lista = new List<CatalogoPermisosDTO>();
            var secciones = await _seccionService.ObtenerTodos();
            var claims = await _claimService.ObtenerTodos();
            foreach(var seccion in secciones)
            {
                var claimsPorSeccion = claims.Where(c => c.IdSeccion == seccion.Id).ToList();
                lista.Add(new CatalogoPermisosDTO
                {
                    Descripcion = seccion.Descripcion,
                    Id = seccion.Id,
                    Nombre = seccion.Nombre,
                    Claims = claimsPorSeccion
                });
            }
            return lista;
        }
    }
}
