namespace API_SSO.DTO
{
    public class LogDTO
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public int IdEmpresa { get; set; }

        public bool EsSso { get; set; }

        public DateTime Fecha { get; set; }

        public string Nivel { get; set; } = null!;

        public string Metodo { get; set; } = null!;

        public string? Descripcion { get; set; }

        public bool? Eliminado { get; set; }
    }
}
