using Microsoft.AspNetCore.Identity;

namespace EventManager.Domain.Models;


public class User : IdentityUser<Guid>
{
    public const int MIN_NAME_LENGTH = 2;
    public const int MAX_NAME_LENGTH = 50;

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }

    public static User Create(string email, string firstName, string lastName, DateTime dateOfBirth)
    {
        if (firstName.Trim().Length < MIN_NAME_LENGTH || firstName.Trim().Length > MAX_NAME_LENGTH)
            throw new ArgumentException(
                $"First name must be between {MIN_NAME_LENGTH} and {MAX_NAME_LENGTH} characters",
                nameof(firstName));

        if (lastName.Trim().Length < MIN_NAME_LENGTH || lastName.Trim().Length > MAX_NAME_LENGTH)
            throw new ArgumentException(
                $"Last name must be between {MIN_NAME_LENGTH} and {MAX_NAME_LENGTH} characters",
                nameof(lastName));

        return new User
        {
            Email = email,
            UserName = email,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            DateOfBirth = dateOfBirth
        };
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName}";
    }
}