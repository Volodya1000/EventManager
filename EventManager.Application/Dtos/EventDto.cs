namespace EventManager.Application.Dtos;

public record EventDto(
       Guid Id,
       string Name,
       string Description,
       DateTime DateTime,
       string Location,
       string Category,
       int MaxParticipants,
       int RegisteredParticipants,
       List<string> ImageUrls,
       List<Guid> ParticipantsIds);
