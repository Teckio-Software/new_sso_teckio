namespace API_SSO.DTO
{
    public class CatalogoClaimDTO
    {
        public int Id { get; set; }

        public string? Nombre { get; set; }

        public string? Descripcion { get; set; }

        public string? CodigoClaim { get; set; }

        public int? IdSeccion { get; set; }

        public string? ClaimType { get; set; }
    }
}
