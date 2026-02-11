namespace Auth.Features.Memberships.Api.MembershipAggregate.Queries;

public class GetMembership : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/memberships/{id}", Handler);
    }

    public interface IService
    {
        Task<MembershipResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(IRepository repository) : IService
    {
        public async Task<MembershipResponse> HandleAsync(Guid id)
        {
            var entity = await repository.Get(id);
            return MembershipResponse.Map(entity);
        }
    }

    [AsNoTracking]
    [Include<Membership>("User", "Role")]
    public interface IRepository : IGet<Membership, Guid> { }
}
