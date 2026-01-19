using System;
using System.Collections.Generic;

namespace API_SSO.Models;

public partial class EmpresaXcliente
{
    public int Id { get; set; }

    public int IdCliente { get; set; }

    public int IdEmpresa { get; set; }

    public bool? Eliminado { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Empresa IdEmpresaNavigation { get; set; } = null!;
}
