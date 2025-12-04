using Microsoft.AspNetCore.Routing;

namespace Fudie.Features;

public interface IFeatureModule
{
    void AddRoutes(IEndpointRouteBuilder app);
}
