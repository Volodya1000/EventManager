using EventManager.Api.Endpoints;

namespace EventManager.Api.Extensions;

public static class EndpointExtensions
{
    public static void AddMappedEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapUserEndpoints();
        app.MapEventEndpoints();
        app.MapParticipantEndpoints();
        app.MapImagesEndpoints();
        app.MapCategoriesEndpoints();
    }
}
