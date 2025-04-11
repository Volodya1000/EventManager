using EventManager.Domain.Enums;

namespace EventManager.Domain.Requests;

public record RegisterRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public DateTime DateOfBirth { get; init; }
    public Role Role { get; init; }
}