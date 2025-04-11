using Microsoft.AspNetCore.Identity;

namespace EventManager.Domain.Models;


public class User : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }

    public static User Create(string email, string firstName, string lastName, DateTime dateOfBirth)
    {
        return new User
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth=dateOfBirth
        };
    }

    public override string ToString()
    {
        return FirstName + " " + LastName;
    }
}
