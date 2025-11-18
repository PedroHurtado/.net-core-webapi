namespace webapi.common;
public static class RouteExtension
{
    public static void MapFeatures(this IEndpointRouteBuilder builder)
    {
        var features = typeof(IFeatureModule).Assembly
            .GetTypes()
            .Where(p => p.IsClass && p.IsPublic && p.IsAssignableTo(typeof(IFeatureModule)))
            .Select(Activator.CreateInstance)
            .Cast<IFeatureModule>();

        /*var authorizedGroup = builder.MapGroup(string.Empty)
            .RequireAuthorization();*/
            
        foreach (var feature in features)
        {
            feature.AddRoutes(builder);
        }
    }
}