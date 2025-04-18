namespace EventManager.Application.Exceptions;

public class UserAlreadyAdminException(string email, string role)
    : Exception($"User with email:'{email}' already has role '{role}'");
