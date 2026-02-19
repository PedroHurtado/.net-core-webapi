namespace Auth.Infrastructure.Memberships;

[Injectable]
public class MembershipLookupService(IQuery query) : IMembershipLookup
{
    public async Task<Membership?> FindFirstByUserId(Guid userId)
    {
        return await query.Query<Membership>()
            .IgnoreQueryFilters()
            .Include(m => m.Role)
            .Where(m => m.IsActive && m.User!.Id == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Membership>> FindAllByUserId(Guid userId)
    {
        return await query.Query<Membership>()
            .IgnoreQueryFilters()
            .Include(m => m.Role)
            .Where(m => m.IsActive && m.User!.Id == userId)
            .ToListAsync();
    }
}
