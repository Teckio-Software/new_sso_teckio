using API_SSO.Servicios.Contratos;
using Azure.Identity;
using System.Net.Mail;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace API_SSO.Servicios
{
    public class EmailService : IEmailService
    {
        private readonly GraphServiceClient _graphServiceClient;

        public EmailService(string tenantId, string clientId, string clientSecret)
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graphServiceClient = new GraphServiceClient(credential);
        }

        public async Task EnviarHtml(string from, string to, string subject, string html, CancellationToken cancellationToken = default)
        {
            var mensaje = new Message
            {
                Subject = subject,
                Body = new ItemBody { ContentType = BodyType.Html, Content = html },
                ToRecipients = [
                    new Recipient { EmailAddress = new EmailAddress { Address = to } }
                ]
            };

            var cuerpo = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = mensaje,
                SaveToSentItems = true
            };

            await _graphServiceClient.Users[from].SendMail.PostAsync(cuerpo, cancellationToken: cancellationToken);
        }
    }
}
