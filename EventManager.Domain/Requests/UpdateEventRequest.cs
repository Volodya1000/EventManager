namespace EventManager.Domain.Requests;

public record UpdateEventRequest(
string Description,
DateTime DateTime,
string Location,
int MaxParticipants,
List<string> ImageUrls);

