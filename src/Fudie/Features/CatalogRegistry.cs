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

    public void Register(string className, Endpoint endpoint)
    {
        var displayName = endpoint.DisplayName ?? className;

        var isExcluded = endpoint.Metadata
            .GetMetadata<ExcludeFromDescriptionAttribute>() is not null
            || endpoint.Metadata
            .GetMetadata<AllowAnonymousAttribute>() is not null;

        var httpMethod = endpoint.Metadata
            .GetMetadata<HttpMethodMetadata>()
            ?.HttpMethods.FirstOrDefault();

        _entries[displayName] = new CatalogEntry(
            DisplayName: displayName,
            ClassName: className,
            HttpVerb: httpMethod ?? "GET",
            IsPlatform: endpoint.Metadata.GetMetadata<PlatformRequirement>() is not null,
            IsInternal: endpoint.Metadata.GetMetadata<InternalRequirement>() is not null,
            IsExcluded: isExcluded,
            CustomGroup: endpoint.Metadata.GetMetadata<GroupRequirement>()?.Group);
    }

    public string? FindClassName(Endpoint endpoint)
        => endpoint.DisplayName is not null
            && _entries.TryGetValue(endpoint.DisplayName, out var entry) ? entry.ClassName : null;

    public int EndpointMapCount => _entries.Count;

    public IReadOnlyList<CatalogEntry> GetAll()
        => _entries.Values.Where(e => !e.IsExcluded).ToList().AsReadOnly();

    public IReadOnlyList<CatalogEntry> GetTenant()
        => _entries.Values.Where(e => !e.IsExcluded && !e.IsPlatform && !e.IsInternal).ToList().AsReadOnly();
}
