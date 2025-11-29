using Microsoft.AspNetCore.Routing;

namespace Fudie;

public interface IFeatureModule
{
    void AddRoutes(IEndpointRouteBuilder app);
}
