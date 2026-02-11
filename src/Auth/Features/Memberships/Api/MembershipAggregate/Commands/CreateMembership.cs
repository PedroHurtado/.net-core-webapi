namespace Auth.Features.Memberships.Api.MembershipAggregate.Commands;

public class CreateMembership : IFeatureModule
{
    public record Request(string InvitationEmail, Guid RoleId);

    public static Func<IService, Request, Task<IResult>> Handler =>
        async (service, request) =>
        {
            var response = await service.HandleAsync(request);
            return Results.Created($"/memberships/{response.Id}", response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/memberships", Handler);
    }

    public interface IService
    {
        Task<MembershipResponse> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        Guid tenantId,
        Membership.Create createMembership,
        IRepository repository,
        IEntityLookup entityLookup,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<MembershipResponse> HandleAsync(Request request)
        {
            var role = await entityLookup.GetRequiredAsync<TenantRole, Guid>(request.RoleId, tracking: false);

            var duplicate = await repository.ExistsByInvitationEmail(request.InvitationEmail);
            ConflictGuard.ThrowIf(duplicate, "A membership with this email already exists in this tenant");

            var command = new CreateMembershipCommand(
                TenantId: tenantId,
                InvitationEmail: request.InvitationEmail,
                Role: role);

            var entity = createMembership.Execute(command);

            repository.Add(entity);
            await unitOfWork.SaveChangesAsync();

            return MembershipResponse.Map(entity);
        }
    }

    public interface IRepository : IAdd<Membership>
    {
        [AsNoTracking]
        Task<bool> ExistsByInvitationEmail(string invitationEmail);
    }
}