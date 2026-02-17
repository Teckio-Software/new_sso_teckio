using System;
using System.Collections.Generic;

namespace API_SSO.Models;

public partial class UsuarioXempresa
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int IdEmpresa { get; set; }

    public bool? Eliminado { get; set; }
    public bool Activo { get; set; }

    public virtual Empresa IdEmpresaNavigation { get; set; } = null!;

}
