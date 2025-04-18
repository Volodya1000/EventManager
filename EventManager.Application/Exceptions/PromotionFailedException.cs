namespace EventManager.Application.Exceptions;

public class PromotionFailedException(string email, string targetRole, string errors)
    : Exception($"Failed to promote user with email:'{email}' to role '{targetRole}'. Errors: {errors}");
