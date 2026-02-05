namespace API_SSO.DTO
{
    public class RolDTO
    {
        public int Id { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public string? Descripcion { get; set; }

        public string? Color { get; set; }

        public int? IdEmpresa { get; set; }

        public string? IdAspNetRole { get; set; }

        public bool? DeSistema { get; set; }

        public bool? General { get; set; }

        public bool? Activo { get; set; }
    }

    public class RolResultDTO : RolDTO
    {
        public string Nombre { get; set; }
    }
}
