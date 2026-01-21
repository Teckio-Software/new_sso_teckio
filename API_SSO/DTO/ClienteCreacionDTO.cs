namespace API_SSO.DTO
{
    public class ClienteCreacionDTO
    {
        //Datos usuario
        public string? NombreUsuario { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? Contrasena { get; set; }
        //Datos empresa
        public string? NombreEmpresa { get; set; }
        public string? RfcEmpresa { get; set; }
        // tipodePersona: string;
        public string? TipoDeProyecto { get; set; }
        // tipoRegimen: string;
        public string? Sociedad { get; set; }
        public string? CpEmpresa { get; set; }
        //Datos proyecto
        public string? CodigoProyecto { get; set; }
        public string? NombreProyecto { get; set; }
        public string? UbicacionProyecto { get; set; }
        public decimal Anticipo { get; set; }
        public int Cp { get; set; }
        public decimal IVA { get; set; }
        public string? Divisa { get; set; }
        public bool EsSabado { get; set; }
        public bool EsDomingo { get; set; }
        public string? FechaInicio { get; set; }
        public string? FechaFin { get; set; }
        //Datos roles
        public List<RolCreacionDTO> roles { get; set; }
        //Datos invitaciones
        public List<UsuarioInvitaciones> invitaciones { get; set; }
    }

    public class UsuarioInvitaciones
    {
        public string? nombreInvitado { get; set; }
        public string? correoInvitado { get; set; }
        public int rolInvitado { get; set; }
    }

    public class RolCreacionDTO
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? Color { get; set; }

    }

    public class ClienteConComprobanteDTO
    {
        public string? RazonSocial { get; set; }
        public string? Correo { get; set; }
        public DateTime DiaPago { get; set; }
        public int CantidadEmpresas { get; set; }
        public int CostoXUsuario { get; set; }
        public int CantidadUsuariosXEmpresa { get; set; }
        public IFormFile Comprobante { get; set; }
    }
}

