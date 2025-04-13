namespace EventManager.Application.Dtos;

public class ParticipantDto
{
    public Guid UserId { get; init; }
    public Guid EventId { get; init; }
    public DateTime RegistrationDate { get; init; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }

    public ParticipantDto(){}
}
