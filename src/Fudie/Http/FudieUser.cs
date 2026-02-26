using System.Security.Claims;
using Fudie.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Fudie.Http;

[Injectable]
public class FudieUser(IHttpContextAccessor httpContextAccessor) : IFudieUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId =>
        User?.FindFirst("sub")?.Value is { } sub && Guid.TryParse(sub, out var id) ? id : null;

    public Guid? TenantId =>
        User?.FindFirst("tid")?.Value is { } tid && Guid.TryParse(tid, out var id) ? id : null;

    public bool IsOwner => User?.HasClaim("owner", "true") ?? false;
}
