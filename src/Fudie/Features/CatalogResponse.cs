namespace Fudie.Features;

public record CatalogResponse(
    string ServiceId,
    IReadOnlyList<CatalogEntry> Scopes);
