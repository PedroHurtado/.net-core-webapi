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

        var allTypes = assemblies
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
            .ToList();

        var aggregateDescriptions = allTypes
            .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic
                && t.IsAssignableTo(typeof(IAggregateDescription))
                && t.GetConstructor(Type.EmptyTypes) is not null)
            .Select(t => (Type: t, Instance: (IAggregateDescription)Activator.CreateInstance(t)!))
            .ToDictionary(x => x.Type.Namespace!, x => x.Instance);

        var features = allTypes
            .Where(p => p.IsClass && !p.IsAbstract && p.IsPublic && p.IsAssignableTo(typeof(IFeatureModule)))
            .Select(Activator.CreateInstance)
            .Cast<IFeatureModule>();

        var catalog = builder.ServiceProvider.GetRequiredService<ICatalogRegistry>();

        foreach (var feature in features)
        {
            var aggregate = ResolveAggregate(feature, aggregateDescriptions);

            var countBefore = builder.DataSources
                .SelectMany(ds => ds.Endpoints).Count();

            feature.AddRoutes(builder);

            var className = feature.GetType().Name;

            foreach (var endpoint in builder.DataSources
                .SelectMany(ds => ds.Endpoints).Skip(countBefore))
            {
                catalog.Register(className, endpoint, aggregate);
            }
        }
    }

    internal static string ExtractAggregateNamespace(string featureNamespace)
    {
        var lastDot = featureNamespace.LastIndexOf('.');
        if (lastDot <= 0)
            throw new InvalidOperationException(
                $"Cannot extract aggregate namespace from '{featureNamespace}'. " +
                $"Expected pattern: {{Project}}.Features.{{Feature}}.Api.{{Aggregate}}Aggregate.{{Commands|Queries}}");

        return featureNamespace[..lastDot];
    }

    private static IAggregateDescription ResolveAggregate(
        IFeatureModule feature,
        Dictionary<string, IAggregateDescription> aggregateDescriptions)
    {
        var featureNamespace = feature.GetType().Namespace
            ?? throw new InvalidOperationException(
                $"IFeatureModule '{feature.GetType().Name}' has no namespace.");

        var aggregateNamespace = ExtractAggregateNamespace(featureNamespace);

        if (aggregateDescriptions.TryGetValue(aggregateNamespace, out var description))
            return description;

        throw new InvalidOperationException(
            $"No IAggregateDescription found for namespace '{aggregateNamespace}'. " +
            $"Feature: {feature.GetType().FullName}. " +
            $"Expected an IAggregateDescription implementation in namespace '{aggregateNamespace}'. " +
            $"Create a class implementing IAggregateDescription in the aggregate folder.");
    }
}
