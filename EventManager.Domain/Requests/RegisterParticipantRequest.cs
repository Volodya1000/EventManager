namespace EventManager.Domain.Requests;

public record RegisterParticipantRequest
{
    public Guid UserId { get; init; }
    public Guid EventId { get; init; }
    public DateTime RegistrationDate { get; init; }
}
