namespace Fudie.Gateway.Catalog;

public interface ICatalogService
{
    [Get("/catalog/anonymous")]
    Task<IApiResponse<List<AnonymousRoute>>> GetAnonymousRoutes();
}
