using API_SSO.Modelos;

namespace API_SSO.DTO
{
    public class CatalogoPermisosDTO: CatalogoSeccionDTO
    {
        public List<CatalogoClaimDTO> Claims {  get; set; } = new List<CatalogoClaimDTO>();
    }
}
