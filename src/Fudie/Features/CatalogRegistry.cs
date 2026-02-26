using Fudie.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Fudie.Features;

[Injectable(ServiceLifetime.Singleton)]
public class CatalogRegistry : ICatalogRegistry
{
    private readonly Dictionary<string, CatalogEntry> _entries = [];
    private readonly Dictionary<string, Endpoint> _endpointMap = [];
    private readonly Dictionary<string, string> _classNameMap = [];

    public void Register(string className, Endpoint endpoint, IAggregateDescription aggregate)
    {
        var displayName = endpoint.DisplayName ?? className;
        var metadata = endpoint.Metadata;

        _endpointMap[displayName] = endpoint;
        _classNameMap[displayName] = className;

        if (metadata.GetMetadata<ExcludeFromDescriptionAttribute>() is not null)
            return;

        var httpMethod = metadata
            .GetMetadata<HttpMethodMetadata>()
            ?.HttpMethods.FirstOrDefault() ?? "GET";

        var groupRequirement = metadata.GetMetadata<GroupRequirement>();

        var scope = groupRequirement?.Group
            ?? (httpMethod is "GET" ? $"{aggregate.Id}:read" : $"{aggregate.Id}:write");

        var scopeDescription = groupRequirement is not null
            ? (groupRequirement.Description ?? groupRequirement.Group)
            : (httpMethod is "GET" ? aggregate.ReadDescription : aggregate.WriteDescription);

        var routePattern = endpoint is RouteEndpoint routeEndpoint
            ? routeEndpoint.RoutePattern.RawText ?? ""
            : "";

        _entries[displayName] = new CatalogEntry(
            ClassName: className,
            HttpVerb: httpMethod,
            RoutePattern: routePattern,
            IsAnonymous: metadata.GetMetadata<AllowAnonymousAttribute>() is not null,
            IsAuthenticated: metadata.GetMetadata<AuthenticatedRequirement>() is not null,
            IsInternal: metadata.GetMetadata<InternalRequirement>() is not null,
            IsPlatform: metadata.GetMetadata<PlatformRequirement>() is not null,
            Description: metadata.GetMetadata<CatalogDescription>()?.Description,
            AggregateId: aggregate.Id,
            AggregateDisplayName: aggregate.DisplayName,
            AggregateIcon: aggregate.Icon,
            AggregateReadDescription: aggregate.ReadDescription,
            AggregateWriteDescription: aggregate.WriteDescription,
            Scope: scope,
            ScopeDescription: scopeDescription);
    }

    public Endpoint? FindEndpoint(string displayName)
        => _endpointMap.GetValueOrDefault(displayName);

    public string? FindClassName(Endpoint endpoint)
        => endpoint.DisplayName is not null
            && _classNameMap.TryGetValue(endpoint.DisplayName, out var name) ? name : null;

    public int EndpointMapCount => _entries.Count;

    public IReadOnlyList<CatalogEntry> GetAll()
        => _entries.Values.ToList().AsReadOnly();

    public IReadOnlyList<CatalogEntry> GetTenant()
        => _entries.Values.Where(e => !e.IsPlatform && !e.IsInternal).ToList().AsReadOnly();
}
