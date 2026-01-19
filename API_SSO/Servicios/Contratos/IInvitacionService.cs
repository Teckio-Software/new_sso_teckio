using API_SSO.Modelos;

namespace API_SSO.Servicios.Contratos
{
    public interface IInvitacionService
    {
        Task<Guid> CrearYEnviar(string email, CancellationToken ct);
        Task<(bool ok, Invitacion? invitacion, string? error)> RedeemAsync(string token, CancellationToken ct);
        Task SeCompleto(Guid invitationId, CancellationToken ct);
        Task<Guid> InvitarUsuario(string email, CancellationToken ct);
    }
}
