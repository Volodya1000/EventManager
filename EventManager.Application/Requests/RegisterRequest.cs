namespace EventManager.Application.Requests;

public record RegisterRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public DateTime DateOfBirth { get; init; }
}