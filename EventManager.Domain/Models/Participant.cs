namespace EventManager.Domain.Models;

public class Participant
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid EventId { get; init; }
    public DateTime RegistrationDate { get; init; }
    public Participant(Guid id, Guid userId, Guid eventId, DateTime registrationDate)
    {
        Id = id;
        UserId = userId;
        EventId = eventId;
        RegistrationDate = registrationDate;
    }
}