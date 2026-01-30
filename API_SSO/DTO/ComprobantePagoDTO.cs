namespace API_SSO.DTO
{
    public class ComprobantePagoDTO
    {
        public int Id { get; set; }

        public int IdCliente { get; set; }

        public string UserId { get; set; } = null!;

        public string? Ruta { get; set; }

        public int? Estatus { get; set; }

        public DateTime? FechaCarga { get; set; }

        public string? IdUsuarioAutorizador { get; set; }

        public bool? Eliminado { get; set; }

        public int? IdEmpresa { get; set; }
    }
}
