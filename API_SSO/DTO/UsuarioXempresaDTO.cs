namespace API_SSO.DTO
{
    public class UsuarioXempresaDTO
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public int IdEmpresa { get; set; }
        public bool Activo { get; set; }

        public bool? Eliminado { get; set; }
    }
}
