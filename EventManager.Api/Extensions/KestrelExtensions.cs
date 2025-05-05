using System.Security.Cryptography.X509Certificates;

namespace EventManager.Api.Extensions;

public static class KestrelExtensions
{
    public static IWebHostBuilder UseCustomKestrelConfiguration(this IWebHostBuilder builder)
    {
        return builder.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ConfigureHttpsDefaults(httpsOptions =>
            {
                var certPath = Path.Combine("/app/certificates", "aspnetapp.pfx");
                var certPassword = "password"; 

                httpsOptions.ServerCertificate = new X509Certificate2(certPath, certPassword);
            });
        });
    }
}