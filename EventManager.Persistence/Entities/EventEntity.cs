using EventManager.Domain.Models;

namespace EventManager.Persistence.Entities;

public class EventEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; } 
    public DateTime DateTime { get; set; } 
    public required string Location { get; set; } 
    public Guid CategoryId { get; set; }
    public int MaxParticipants { get; set; }
    public ICollection<ImageEntity> ImageUrls { get; set; } = new List<ImageEntity>();

    public CategoryEntity Category { get; set; }
    public ICollection<ParticipantEntity> Participants { get; set; } = new List<ParticipantEntity>();
}
