namespace Fudie.Features;

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
    string? CustomGroupDescription,
    string? Description,
    string AggregateId,
    string AggregateDisplayName,
    string? AggregateIcon,
    string AggregateReadDescription,
    string AggregateWriteDescription);
