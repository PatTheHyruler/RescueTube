using RescueTube.Domain.Enums;

namespace RescueTube.WebApi.Endpoints;

public static class OptionsEndpoints
{
    public static void MapOptionsEndpoints(this IEndpointRouteBuilder app)
    {
        var optionsGroup = app.MapGroup("options").WithTags("Options");

        optionsGroup.MapGet("SupportedPlatforms", () => TypedResults.Ok(Enum.GetValues<EPlatform>()))
            .AllowAnonymous()
            .HasApiVersion(1);
    }
}