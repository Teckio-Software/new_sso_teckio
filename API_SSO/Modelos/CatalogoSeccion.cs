namespace API_SSO.Modelos
{
    public class CatalogoSeccion
    {
        public int Id { get; set; }

        public string? Nombre { get; set; }

        public string? Descripcion { get; set; }

        public int? IdMenu { get; set; }

        public virtual ICollection<CatalogoClaim> CatalogoClaims { get; set; } = new List<CatalogoClaim>();

        public virtual CatalogoMenu? IdMenuNavigation { get; set; }
    }
}
