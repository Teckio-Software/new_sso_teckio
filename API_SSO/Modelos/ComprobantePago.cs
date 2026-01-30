using System;
using System.Collections.Generic;

namespace API_SSO.Models;

public partial class ComprobantePago
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

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Empresa? IdEmpresaNavigation { get; set; }

}
