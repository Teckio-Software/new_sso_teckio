using API_SSO.Models;

namespace API_SSO.DTOs
{
    public class ClienteDTO
    {
        public int Id { get; set; }

        public string? RazonSocial { get; set; }

        public string? Correo { get; set; }

        public int? DiaPago { get; set; }

        public int? CantidadEmpresas { get; set; }

        public int? CantidadUsuariosXempresa { get; set; }

        public decimal? CostoXusuario { get; set; }

        public bool? CorreoConfirmed { get; set; }

        public bool? Eliminado { get; set; }
        public bool Estatus { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
