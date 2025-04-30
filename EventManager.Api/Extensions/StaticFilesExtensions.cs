using Microsoft.Extensions.FileProviders;

namespace EventManager.Api.Extensions;

public static class StaticFilesExtensions
{
    public static void ConfigureStaticFiles(this IApplicationBuilder app)
    {
        var uploadPath = Environment.GetEnvironmentVariable("FILE_STORAGE_PATH") ?? "/data/uploads";
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadPath),
            RequestPath = "/uploads"
        });
    }
}
