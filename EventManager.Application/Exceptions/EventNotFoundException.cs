namespace EventManager.Application.Exceptions;

public class EventNotFoundException(Guid eventId)
    : Exception($"Event with Id: {eventId} not found");