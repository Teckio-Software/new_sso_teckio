using System;
using System.Collections.Generic;

namespace API_SSO.Models;

public partial class Cliente
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

    public virtual ICollection<ComprobantePago> ComprobantePagos { get; set; } = new List<ComprobantePago>();

    public virtual ICollection<EmpresaXcliente> EmpresaXclientes { get; set; } = new List<EmpresaXcliente>();
}
