namespace API_SSO.DTO
{
    public class ProyectoActualDTO
    {
        public int Id { get; set; }

        public int? IdProyecto { get; set; }

        public int IdEmpresa { get; set; }

        public string UserId { get; set; } = null!;

        public bool? Eliminado { get; set; }

    }
}
