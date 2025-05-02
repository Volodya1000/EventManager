namespace EventManager.Application.Requests;

public record CreateEventRequest(
string Name,
string Description,
DateTime DateTime,
string Location,
Guid CategoryId,
int MaxParticipants);
