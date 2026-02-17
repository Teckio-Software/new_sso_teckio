namespace API_SSO.DTO
{
    public class RelacionEmpresaUsuarioDTO
    {
        public int IdEmpresa { get; set; }
        public string NombreEmpresa { get; set; }
        public bool Activo { get; set; }
        public string IdUsuario { get; set; }
    }
}
