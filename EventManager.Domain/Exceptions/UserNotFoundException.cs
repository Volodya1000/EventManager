namespace EventManager.Domain.Exceptions;

public class UserNotFoundException(string email)
    : Exception($"User with email: {email} not found");