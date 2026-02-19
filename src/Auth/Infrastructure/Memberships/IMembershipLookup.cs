namespace Auth.Infrastructure.Memberships;

public interface IMembershipLookup
{
    Task<Membership?> FindFirstByUserId(Guid userId);
    Task<bool> ExistsByUserIdAndTenantId(Guid userId, Guid tenantId);
    Task<List<Membership>> FindAllByUserId(Guid userId);
}
