namespace Fudie.Features;

public record CatalogEntry(
    string ClassName,
    string HttpVerb,
    bool IsPlatform,
    bool IsInternal,
    string? CustomGroup);
