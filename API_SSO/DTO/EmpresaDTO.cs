namespace API_SSO.DTO
{
    public class EmpresaDTO
    {
        public int Id { get; set; }

        public string? NombreComercial { get; set; }

        public string? Rfc { get; set; }

        public bool? Estatus { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public string? CodigoPostal { get; set; }

        public bool? Eliminado { get; set; }
    }
}
