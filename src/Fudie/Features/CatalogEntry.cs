namespace Fudie.Features;

public record CatalogEntry(
    string DisplayName,
    string ClassName,
    string HttpVerb,
    bool IsPlatform,
    bool IsInternal,
    bool IsExcluded,
    string? CustomGroup);
