using Fudie.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Fudie.Features;

[Injectable(ServiceLifetime.Singleton)]
public class CatalogRegistry : ICatalogRegistry
{
    private readonly Dictionary<string, CatalogEntry> _entries = [];
    private readonly Dictionary<string, Endpoint> _endpointMap = [];

    public void Register(string className, Endpoint endpoint, IAggregateDescription aggregate)
    {
        var displayName = endpoint.DisplayName ?? className;

        var isAnonymous = endpoint.Metadata
            .GetMetadata<AllowAnonymousAttribute>() is not null;

        var isExcluded = endpoint.Metadata
            .GetMetadata<ExcludeFromDescriptionAttribute>() is not null;

        var httpMethod = endpoint.Metadata
            .GetMetadata<HttpMethodMetadata>()
            ?.HttpMethods.FirstOrDefault();

        var routePattern = endpoint is RouteEndpoint routeEndpoint
            ? routeEndpoint.RoutePattern.RawText ?? ""
            : "";

        var groupRequirement = endpoint.Metadata.GetMetadata<GroupRequirement>();

        _entries[displayName] = new CatalogEntry(
            ClassName: className,
            HttpVerb: httpMethod ?? "GET",
            RoutePattern: routePattern,
            IsAnonymous: isAnonymous,
            IsAuthenticated: endpoint.Metadata.GetMetadata<AuthenticatedRequirement>() is not null,
            IsInternal: endpoint.Metadata.GetMetadata<InternalRequirement>() is not null,
            IsPlatform: endpoint.Metadata.GetMetadata<PlatformRequirement>() is not null,
            IsExcluded: isExcluded,
            CustomGroup: groupRequirement?.Group,
            CustomGroupDescription: groupRequirement?.Description,
            Description: endpoint.Metadata.GetMetadata<CatalogDescription>()?.Description,
            AggregateId: aggregate.Id,
            AggregateDisplayName: aggregate.DisplayName,
            AggregateIcon: aggregate.Icon,
            AggregateReadDescription: aggregate.ReadDescription,
            AggregateWriteDescription: aggregate.WriteDescription);

        _endpointMap[displayName] = endpoint;
    }

    public Endpoint? FindEndpoint(string displayName)
        => _endpointMap.GetValueOrDefault(displayName);

    public string? FindClassName(Endpoint endpoint)
        => endpoint.DisplayName is not null
            && _entries.TryGetValue(endpoint.DisplayName, out var entry) ? entry.ClassName : null;

    public int EndpointMapCount => _entries.Count;

    public IReadOnlyList<CatalogEntry> GetAll()
        => _entries.Values.Where(e => !e.IsExcluded).ToList().AsReadOnly();

    public IReadOnlyList<CatalogEntry> GetTenant()
        => _entries.Values.Where(e => !e.IsExcluded && !e.IsPlatform && !e.IsInternal).ToList().AsReadOnly();
}
