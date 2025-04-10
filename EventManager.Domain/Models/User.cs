using Microsoft.AspNetCore.Identity;

namespace EventManager.Domain.Models;

public class User : IdentityUser<Guid>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public DateTime DateOfBirth { get; init; }
    public required string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }

    public User(
        Guid id,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string email,
        string passwordHash)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Email = email;
        PasswordHash = passwordHash;
    }
}
