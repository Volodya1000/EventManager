namespace EventManager.Domain.Models;

public class User
{
    public Guid Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string Email { get; init; }
    public string PasswordHash { get; init; }

    public User(
        Guid id,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string email,
        string passwordHash)
    {
        Id=id;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Email = email;
        PasswordHash = passwordHash;
    }
}
