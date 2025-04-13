namespace EventManager.Application.Requests;

public record UpdateEventRequest(
string Description,
DateTime DateTime,
string Location,
int MaxParticipants);

