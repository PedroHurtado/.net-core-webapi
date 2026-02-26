namespace Fudie.Http;

public interface IFudieUser
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    bool IsOwner { get; }
    bool IsAuthenticated { get; }
}
