namespace API_SSO.DTO
{
    public class UsuarioDTO
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string IdRol { get; set; }
        public string Rol { get; set; }

    }

    public class UsuarioBaseDTO
    {
        public string NombreUsuario { get; set; }
        public string Correo { get; set; }
        public string Password { get; set; }
        public int IdEmpresa { get; set; }
    }

    public class OperativoBaseDTO
    {
        public string NombreUsuario { get; set; }
        public string Correo { get; set; }
        public int IdRol { get; set; }
        public int IdEmpresa { get; set; }
    }
}
