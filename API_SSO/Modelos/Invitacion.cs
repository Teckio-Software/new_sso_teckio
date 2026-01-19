namespace API_SSO.Modelos
{
    public class Invitacion
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? RedeemedAt { get; set; }
        public string TokenJti { get; set; } = default!;
    }
}
