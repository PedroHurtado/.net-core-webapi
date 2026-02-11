namespace Auth.Features.Memberships.Api.MembershipAggregate.Queries;

public class GetMemberships : IFeatureModule
{
    public static Func<IService, Task<IResult>> Handler =>
        async (service) =>
        {
            var response = await service.HandleAsync();
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/memberships", Handler);
    }

    public interface IService
    {
        Task<List<MembershipResponse>> HandleAsync();
    }

    [Injectable]
    public class Service(IQuery query) : IService
    {
        public async Task<List<MembershipResponse>> HandleAsync()
        {
            var memberships = await query.Query<Membership>()
                .Include(x => x.User)
                .Include(x => x.Role)
                .OrderBy(x => x.InvitationEmail)
                .ToListAsync();

            return [.. memberships.Select(MembershipResponse.Map)];
        }
    }
}
