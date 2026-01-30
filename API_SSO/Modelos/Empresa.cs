using API_SSO.Modelos;
using System;
using System.Collections.Generic;

namespace API_SSO.Models;

public partial class Empresa
{
    public int Id { get; set; }

    public string? NombreComercial { get; set; }

    public string? Rfc { get; set; }

    public bool? Estatus { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public string? CodigoPostal { get; set; }

    public bool? Eliminado { get; set; }

    public string Sociedad { get; set; } = null!;

    public int? DiaPago { get; set; }

    public virtual ICollection<ComprobantePago> ComprobantePagos { get; set; } = new List<ComprobantePago>();

    public virtual ICollection<EmpresaXcliente> EmpresaXclientes { get; set; } = new List<EmpresaXcliente>();

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual ICollection<ProyectoActual> ProyectoActuals { get; set; } = new List<ProyectoActual>();

    public virtual ICollection<Rol> Rols { get; set; } = new List<Rol>();

    public virtual ICollection<UsuarioXempresa> UsuarioXempresas { get; set; } = new List<UsuarioXempresa>();
}
