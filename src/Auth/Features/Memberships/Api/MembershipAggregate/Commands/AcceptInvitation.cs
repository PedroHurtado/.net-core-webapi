namespace Auth.Features.Memberships.Api.MembershipAggregate.Commands;

public class AcceptInvitation : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/memberships/{id}/accept", Handler);
    }

    public interface IService
    {
        Task<MembershipResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        CurrentUserId currentUserId,
        Membership.AcceptInvitation acceptInvitation,
        IRepository repository,
        IEntityLookup entityLookup,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MembershipResponse> HandleAsync(Guid id)
        {
            var entity = await repository.Get(id);
            var user = await entityLookup.GetRequiredAsync<User, Guid>(currentUserId.Value, tracking: false);

            acceptInvitation.Execute(entity, new AcceptInvitationCommand(User: user));

            await unitOfWork.SaveChangesAsync();

            return MembershipResponse.Map(entity);
        }
    }

    [Include<Membership>("User", "Role")]
    public interface IRepository : IUpdate<Membership, Guid> { }
}
