namespace Auth.Infrastructure.Memberships;

public interface IMembershipLookup
{
    Task<Membership?> FindFirstByUserId(Guid userId);
    Task<List<Membership>> FindAllByUserId(Guid userId);
}
