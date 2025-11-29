using Microsoft.AspNetCore.Routing;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fudie;

public static class RouteExtension
{
    public static void MapFeatures(this IEndpointRouteBuilder builder)
    {
        // Obtener el ensamblado del llamador basándose en el archivo
        var callingAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

        var features = callingAssembly
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
