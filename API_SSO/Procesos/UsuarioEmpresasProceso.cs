using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios.Contratos;
using System.Security.Claims;

namespace API_SSO.Procesos
{
    public class UsuarioEmpresasProceso
    {
        private readonly IUsuarioxEmpresaService<SSOContext> _usuarioxEmpresaService;
        private readonly IEmpresaService<SSOContext> _empresaService;

        public UsuarioEmpresasProceso(IUsuarioxEmpresaService<SSOContext> usuarioxEmpresaService, IEmpresaService<SSOContext> empresaService)
        {
            _usuarioxEmpresaService = usuarioxEmpresaService;
            _empresaService = empresaService;
        }

        public async Task<List<EmpresaDTO>> ObtenerEmpresasXUsuario(string idUsuario)
        {
            var relaciones = await _usuarioxEmpresaService.ObtenerXIdUsuario(idUsuario);
            List<EmpresaDTO> empresas = new List<EmpresaDTO>();
            foreach (var relacion in relaciones)
            {
                var empresa = await _empresaService.ObtenerXId(relacion.IdEmpresa);
                empresas.Add(empresa);
            }

            return empresas;
        }

        public async Task<List<EmpresaDTO>> ObtenerEmpresasPerteneciente(List<Claim> claims)
        {
            var idUsuario = claims.FirstOrDefault(c => c.Type == "guid")?.Value;
           if (idUsuario == null)
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

            return empresas;
        }
    }
}
