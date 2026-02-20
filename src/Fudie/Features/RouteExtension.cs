using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Fudie.Features;

public static class RouteExtension
{
    public static void MapFeatures(this IEndpointRouteBuilder builder)
    {
        var interfaceAssembly = typeof(IFeatureModule).Assembly;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic &&
                       (a == interfaceAssembly ||
                        a.GetReferencedAssemblies().Any(ra => ra.Name == interfaceAssembly.GetName().Name)))
            .ToList();

        var features = assemblies
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    return [];
                }
            })
            .Where(p => p.IsClass && !p.IsAbstract && p.IsPublic && p.IsAssignableTo(typeof(IFeatureModule)))
            .Select(Activator.CreateInstance)
            .Cast<IFeatureModule>();

        var catalog = builder.ServiceProvider.GetRequiredService<ICatalogRegistry>();

        foreach (var feature in features)
        {
            var countBefore = builder.DataSources
                .SelectMany(ds => ds.Endpoints).Count();

            feature.AddRoutes(builder);

            var className = feature.GetType().Name;

            foreach (var endpoint in builder.DataSources
                .SelectMany(ds => ds.Endpoints).Skip(countBefore))
            {
                catalog.Register(className, endpoint);
            }
        }
    }
}
