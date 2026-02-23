namespace Fudie.Gateway.Catalog;

public interface ICatalogService
{
    [Get("/catalog")]
    Task<IApiResponse<CatalogResponse>> GetCatalog();
}
