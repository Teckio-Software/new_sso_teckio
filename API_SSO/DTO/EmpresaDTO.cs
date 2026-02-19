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

        public string Sociedad { get; set; }

        public int? DiaPago { get; set; }

        public string? Ciudad { get; set; }

        public string? Domicilio { get; set; }
    }

    public class EmpresaCreacionDTO: EmpresaDTO
    {
        public bool PagoMismoDia {  get; set; }
        public int IdCliente { get; set; }
    }
}
