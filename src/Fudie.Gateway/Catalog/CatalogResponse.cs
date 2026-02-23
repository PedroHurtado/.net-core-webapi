namespace Fudie.Gateway.Catalog;

public record CatalogResponse(
    string ServiceId,
    string ServiceName,
    List<CatalogEntry> Entries);
