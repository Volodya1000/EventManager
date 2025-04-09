using Microsoft.AspNet.Identity.EntityFramework;

namespace EventManager.Persistence.Entities;

public class UserEntity : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }

    public ICollection<ParticipantEntity> Participants { get; set; } = new List<ParticipantEntity>();
}
