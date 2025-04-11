namespace EventManager.Domain.Constants;

public static class IdentityRoleConstants
{
    public static readonly Guid AdminRoleGuid = new("5d8d3cc8-4fde-4c21-a70b-deaf8ebe51a2");
    public static readonly Guid UserGuid = new("66dddd1c-bf05-4032-b8a5-6adbf73dc09e");

    public const string Admin = nameof(Admin);
    public const string User = nameof(User);
}