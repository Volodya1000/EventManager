using Microsoft.AspNetCore.Identity;

namespace EventManager.Persistence.Entities;

public class UserEntity : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; } 
    public DateTime DateOfBirth { get; set; }
    public required  string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }

    public ICollection<ParticipantEntity> Participants { get; set; } = new List<ParticipantEntity>();
}
