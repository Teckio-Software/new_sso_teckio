namespace API_SSO.Modelos
{
    public class CatalogoClaim
    {
        public int Id { get; set; }

        public string? Nombre { get; set; }

        public string? Descripcion { get; set; }

        public string? CodigoClaim { get; set; }

        public int? IdSeccion { get; set; }

        public virtual CatalogoSeccion? IdSeccionNavigation { get; set; }
    }
}
