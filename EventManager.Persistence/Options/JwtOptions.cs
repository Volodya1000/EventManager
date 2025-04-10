namespace EventManager.Domain.Options;

public class JwtOptions
{
    public const string JwtOptionsKey = "JwtOptions"; //имя соответсвует ключу в appsettings.json

    //имена соответсвуют ключам внутри appsettings.json JwtOptions
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationTimeInMinutes { get; set; }
}