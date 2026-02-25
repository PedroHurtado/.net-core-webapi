namespace Auth.Features.Memberships.Api.MembershipAggregate.Commands;

public class DeactivateMembership : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/memberships/{id}/deactivate", Handler)
            .WithDescriptionCatalog("Deactivate membership");
    }

    public interface IService
    {
        Task<MembershipResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        Membership.Deactivate deactivateMembership,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MembershipResponse> HandleAsync(Guid id)
        {
            var entity = await repository.Get(id);

            deactivateMembership.Execute(entity);

            await unitOfWork.SaveChangesAsync();

            return MembershipResponse.Map(entity);
        }
    }

    [Include<Membership>("User", "Role")]
    public interface IRepository : IUpdate<Membership, Guid> { }
}
