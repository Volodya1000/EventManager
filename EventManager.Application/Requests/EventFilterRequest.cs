namespace EventManager.Application.Requests;

public class EventFilterRequest
{
    public int? AvailableSpaces { get; set; } // Свободные места (>= указанного числа)
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Location { get; set; }
    public List<string>? Categories { get; set; }
    public int? MaxParticipants { get; set; }
}