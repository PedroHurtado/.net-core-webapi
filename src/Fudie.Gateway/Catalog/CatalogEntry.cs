namespace Fudie.Gateway.Catalog;

public record CatalogEntry(
    string ClassName,
    string HttpVerb,
    string RoutePattern,
    bool IsAnonymous,
    bool IsAuthenticated,
    bool IsInternal,
    bool IsPlatform,
    bool IsExcluded,
    string? CustomGroup,
    string? Description);
