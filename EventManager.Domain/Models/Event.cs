namespace EventManager.Domain.Models;

public class Event
{
    public const int MIN_DESCRIPTION_LENGTH = 15;
    public const int MAX_DESCRIPTION_LENGTH = 2000;
    public const int MIN_LOCATION_LENGTH = 10;
    public const int MAX_LOCATION_LENGTH = 200;

    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; private set; }
    public DateTime DateTime { get; private set; }
    public string Location { get; private set; }
    public Guid CategoryId { get; }
    public int MaxParticipants { get; private set; }
    public int RegisteredParticipants => _participants.Count;

    private readonly List<string> _imageUrls;
    public IReadOnlyList<string> ImageUrls => _imageUrls.AsReadOnly();

    private readonly List<Participant> _participants = new();
    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();

    private Event(
        Guid id,
        string name,
        string description,
        DateTime dateTime,
        string location,
        Guid categoryId,
        int maxParticipants,
        List<string> imageUrls)
    {
        Id = id;
        Name = name;
        Description = description;
        DateTime = dateTime;
        Location = location;
        CategoryId = categoryId;
        MaxParticipants = maxParticipants;
        _imageUrls = imageUrls ?? new List<string>();
    }

    public static Event Create(
        Guid id,
        string name,
        string description,
        DateTime dateTime,
        string location,
        Guid categoryId,
        int maxParticipants,
        List<string> imageUrls = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Event name cannot be empty", nameof(name));

        if (maxParticipants <= 0)
            throw new ArgumentException("Max participants must be positive", nameof(maxParticipants));

        if (dateTime < DateTime.UtcNow)
            throw new ArgumentException("Event date cannot be in the past", nameof(dateTime));

        if (description == null)
            throw new ArgumentException("Description cannot be null", nameof(description));

        string trimmedDescription = description.Trim();
        if (trimmedDescription.Length < MIN_DESCRIPTION_LENGTH || trimmedDescription.Length > MAX_DESCRIPTION_LENGTH)
            throw new ArgumentException($"Description must be between {MIN_DESCRIPTION_LENGTH} and {MAX_DESCRIPTION_LENGTH} characters long.", nameof(description));

        string trimmedLocation = location.Trim();
        if (trimmedLocation.Length < MIN_LOCATION_LENGTH || trimmedLocation.Length > MAX_LOCATION_LENGTH)
            throw new ArgumentException($"Location must be between {MIN_LOCATION_LENGTH} and {MAX_LOCATION_LENGTH} characters long.", nameof(location));

        return new Event(
            id,
            name.Trim(),
            trimmedDescription,
            dateTime,
            trimmedLocation,
            categoryId,
            maxParticipants,
            imageUrls);
    }

    public void AddParticipant(Participant participant)
    {
        if (_participants.Count >= MaxParticipants)
            throw new InvalidOperationException("Event is full");

        if (_participants.Any(p => p.UserId == participant.UserId))
            throw new InvalidOperationException("Participant already registered");

        _participants.Add(participant);
    }

    public bool RemoveParticipant(Guid userId)
    {
        var participant = _participants.FirstOrDefault(p => p.UserId == userId) 
            ?? throw new InvalidOperationException("Participation not found");
        return _participants.Remove(participant);
    }

    public void UpdateDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new ArgumentException("Description cannot be empty or whitespace.", nameof(newDescription));

        string trimmedDescription = newDescription.Trim();
        if (trimmedDescription.Length < MIN_DESCRIPTION_LENGTH || trimmedDescription.Length > MAX_DESCRIPTION_LENGTH)
            throw new ArgumentException($"Description must be between {MIN_DESCRIPTION_LENGTH} and {MAX_DESCRIPTION_LENGTH} characters long.", nameof(newDescription));

        Description = trimmedDescription;
    }

    public void UpdateDateTime(DateTime newDateTime)
    {
        if (newDateTime < DateTime.UtcNow)
            throw new ArgumentException("Event date cannot be in the past.", nameof(newDateTime));

        DateTime = newDateTime;
    }

    public void UpdateLocation(string newLocation)
    {
        if (string.IsNullOrWhiteSpace(newLocation))
            throw new ArgumentException("Location cannot be empty or whitespace.", nameof(newLocation));

        string trimmedLocation = newLocation.Trim();
        if (trimmedLocation.Length < MIN_LOCATION_LENGTH || trimmedLocation.Length > MAX_LOCATION_LENGTH)
            throw new ArgumentException($"Location must be between {MIN_LOCATION_LENGTH} and {MAX_LOCATION_LENGTH} characters long.", nameof(newLocation));

        Location = trimmedLocation;
    }

    public void UpdateMaxParticipants(int newMax)
    {
        if (newMax <= 0)
            throw new ArgumentException("Max participants must be positive.", nameof(newMax));

        if (newMax < RegisteredParticipants)
            throw new ArgumentException("New max participants cannot be less than registered participants.", nameof(newMax));

        MaxParticipants = newMax;
    }
}