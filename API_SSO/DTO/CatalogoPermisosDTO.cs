using API_SSO.Modelos;

namespace API_SSO.DTO
{
    public class CatalogoPermisosDTO: CatalogoSeccionDTO
    {
        public List<CatalogoClaimDTO> Claims {  get; set; } = new List<CatalogoClaimDTO>();
    }

    public class CatalogoPermisoMenuDTO: CatalogoMenuDTO
    {
        public List<CatalogoPermisosDTO> Secciones { get; set; } = new List<CatalogoPermisosDTO>();
    }
}
