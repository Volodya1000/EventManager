namespace EventManager.Application.Requests;

public record CreateEventRequest(
string Name,
string Description,
DateTime DateTime,
string Location,
string Category,
int MaxParticipants,
List<string> ImageUrls);
