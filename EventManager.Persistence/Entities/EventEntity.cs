namespace EventManager.Persistence.Entities;

public class EventEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } 
    public string Location { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public int MaxParticipants { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    public CategoryEntity Category { get; set; }
    public ICollection<ParticipantEntity> Participants { get; set; } = new List<ParticipantEntity>();
}
