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
    private readonly List<CatalogEntry> _entries = [];

    public void Register(string className, Endpoint endpoint)
    {
        var hasExclude = endpoint.Metadata
            .GetMetadata<ExcludeFromDescriptionAttribute>() is not null;

        var hasAllowAnonymous = endpoint.Metadata
            .GetMetadata<AllowAnonymousAttribute>() is not null;

        if (hasExclude || hasAllowAnonymous)
            return;

        var httpMethod = endpoint.Metadata
            .GetMetadata<HttpMethodMetadata>()
            ?.HttpMethods.FirstOrDefault();

        _entries.Add(new CatalogEntry(
            ClassName: className,
            HttpVerb: httpMethod ?? "GET",
            IsPlatform: endpoint.Metadata.GetMetadata<PlatformRequirement>() is not null,
            IsInternal: endpoint.Metadata.GetMetadata<InternalRequirement>() is not null,
            CustomGroup: endpoint.Metadata.GetMetadata<GroupRequirement>()?.Group));
    }

    public IReadOnlyList<CatalogEntry> GetAll()
        => _entries.AsReadOnly();

    public IReadOnlyList<CatalogEntry> GetTenant()
        => _entries.Where(e => !e.IsPlatform && !e.IsInternal).ToList().AsReadOnly();
}
