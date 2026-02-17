using API_SSO.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Servicios.Contratos
{
    public interface IUsuarioxEmpresaService<TContext> where TContext : DbContext
    {
        Task<List<UsuarioXempresaDTO>> ObtenerTodos();
        Task<List<UsuarioXempresaDTO>> ObtenerXIdUsuario(string IdUsuario);
        Task<List<UsuarioXempresaDTO>> ObtenerXIdEmpresa(int IdEmpresa);
        Task<UsuarioXempresaDTO> ObtenerXId(int id);
        Task<UsuarioXempresaDTO> CrearYObtener(UsuarioXempresaDTO usuarioXempresa);
        Task<RespuestaDTO> Editar(UsuarioXempresaDTO usuarioXempresa);
        Task<RespuestaDTO> Eliminar(int id);

    }
}
