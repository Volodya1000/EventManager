namespace EventManager.Domain.Models;

public class Participant
{
    public Guid UserId { get; init; }
    public Guid EventId { get; init; }
    public DateTime RegistrationDate { get; init; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }


    private Participant(
        Guid userId,
        Guid eventId,
        DateTime registrationDate,
        string firstName,
        string lastName,
        DateTime dateOfBirth)
    {
        UserId = userId;
        EventId = eventId;
        RegistrationDate = registrationDate;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
    }

    public static Participant Create(
       Guid userId,
       Guid eventId,
       DateTime registrationDate,
       string firstName,
       string lastName,
       DateTime dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        if (dateOfBirth > DateTime.UtcNow.AddYears(-16))
            throw new ArgumentException("Participant must be at least 16 years old", nameof(dateOfBirth));

        return new Participant(
            userId,
            eventId,
            registrationDate,
            firstName.Trim(),
            lastName.Trim(),
            dateOfBirth);
    }
}