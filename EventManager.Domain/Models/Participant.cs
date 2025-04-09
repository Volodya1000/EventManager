namespace EventManager.Domain.Models;

public class Participant
{
    public int Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string Email { get; private set; }

    public Participant(
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string email)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Email = email;
    }
}