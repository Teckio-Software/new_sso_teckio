namespace API_SSO.Servicios.Contratos
{
    public interface IEmailService
    {
        Task EnviarHtml(string from, string to, string subject, string html, CancellationToken cancellationToken = default);
    }
}
