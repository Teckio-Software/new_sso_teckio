namespace API_SSO.Modelos
{
    public class CatalogoMenu
    {
        public int Id { get; set; }

        public string? Nombre { get; set; }

        public virtual ICollection<CatalogoSeccion> CatalogoSeccions { get; set; } = new List<CatalogoSeccion>();
    }
}
