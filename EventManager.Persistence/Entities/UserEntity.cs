namespace EventManager.Persistence.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<ParticipantEntity> Participants { get; set; } = new List<ParticipantEntity>();
}
