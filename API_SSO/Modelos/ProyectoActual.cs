using System;
using System.Collections.Generic;

namespace API_SSO.Models;

public partial class ProyectoActual
{
    public int Id { get; set; }

    public int? IdProyecto { get; set; }

    public int IdEmpresa { get; set; }

    public string UserId { get; set; } = null!;

    public bool? Eliminado { get; set; }

    public virtual Empresa IdEmpresaNavigation { get; set; } = null!;

}
