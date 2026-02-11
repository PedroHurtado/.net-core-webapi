namespace Auth.Features.Memberships.Api.MembershipAggregate.Commands;

public class ChangeMembershipRole : IFeatureModule
{
    public record Request(Guid RoleId);

    public static Func<IService, Guid, Request, Task<IResult>> Handler =>
        async (service, id, request) =>
        {
            await service.HandleAsync(id, request);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/memberships/{id}/role", Handler);
    }

    public interface IService
    {
        Task HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        Membership.ChangeRole changeRole,
        IRepository repository,
        IEntityLookup entityLookup,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var entity = await repository.Get(id);
            var role = await entityLookup.GetRequiredAsync<TenantRole, Guid>(request.RoleId, tracking: false);

            changeRole.Execute(entity, new ChangeRoleCommand(Role: role));

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Membership, Guid> { }
}
