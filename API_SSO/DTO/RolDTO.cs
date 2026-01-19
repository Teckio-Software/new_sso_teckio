namespace API_SSO.DTO
{
    public class RolDTO
    {
        public int Id { get; set; }

        public string? Nombre { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public string? Descripcion { get; set; }

        public string? Color { get; set; }

        public int? IdEmpresa { get; set; }

        public string? IdAspNetRole { get; set; }

        public bool? Borrado { get; set; }
    }
}
