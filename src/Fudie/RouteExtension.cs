using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Fudie;

public static class RouteExtension
{
    public static void MapFeatures(this IEndpointRouteBuilder builder)
    {
        // Obtener el ensamblado donde está definido IFeatureModule
        var interfaceAssembly = typeof(IFeatureModule).Assembly;

        // Buscar en todos los ensamblados cargados que referencian a Fudie
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic &&
                       (a == interfaceAssembly ||
                        a.GetReferencedAssemblies().Any(ra => ra.Name == interfaceAssembly.GetName().Name)))
            .ToList();

        // Descubrir todos los tipos que implementan IFeatureModule
        var features = assemblies
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    // Ignorar ensamblados que no se pueden cargar
                    return [];
                }
            })
            .Where(p => p.IsClass && !p.IsAbstract && p.IsPublic && p.IsAssignableTo(typeof(IFeatureModule)))
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
