namespace Auth.Features.Users.Api.UserAggregate.Commands;

public class Seed : IFeatureModule
{
    public record Request(
        SeedUserRequest User,
        SeedCustomerRequest Customer);

    public record SeedUserRequest(
        string Email,
        string Name,
        string Password);

    public record SeedCustomerRequest(
        string Name,
        string Slug,
        string? Description,
        string EstablishmentType,
        string DefaultCulture,
        string TimeZoneId,
        SeedAddressRequest Address,
        SeedContactInfoRequest ContactInfo,
        SeedBillingInfoRequest BillingInfo);

    public record SeedAddressRequest(
        string Street,
        string City,
        string PostalCode,
        string Region,
        string Country,
        decimal Latitude,
        decimal Longitude);

    public record SeedContactInfoRequest(
        string Phone,
        string? Email,
        string? WebsiteUrl);

    public record SeedBillingInfoRequest(
        string BusinessName,
        string TaxId,
        SeedAddressRequest BillingAddress);

    public static Func<IService, HttpContext, Request, Task<IResult>> Handler =>
        async (service, httpContext, request) =>
        {
            var seedKey = httpContext.Request.Headers["X-Seed-Key"].FirstOrDefault();
            await service.HandleAsync(seedKey, request);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/seed", Handler)
            .AllowAnonymous()
            .WithDescriptionCatalog("Seed auth data");
    }

    public interface IService
    {
        Task HandleAsync(string? seedKey, Request request);
    }

    [Injectable]
    public class Service(
        IConfiguration configuration,
        User.Create createUser,
        TenantRole.CreateOwnerRole createOwnerRole,
        Membership.Create createMembership,
        Membership.AcceptInvitation acceptInvitation,
        ICustomerApi customerApi,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(string? seedKey, Request request)
        {
            var expectedKey = configuration["Fudie:SeedKey"];
            UnauthorizedGuard.ThrowIf(seedKey is null || seedKey != expectedKey, "Invalid seed key");

            var platformTenantId = Guid.Parse(configuration["Fudie:PlatformTenantId"]!);

            var existingUser = await userRepository.FindFirstByEmailAndProvider(
                request.User.Email, AuthProvider.Local);
            ConflictGuard.ThrowIf(existingUser is not null, "Platform owner already exists");

            // Create User
            var user = createUser.Execute(new CreateUserCommand(
                ProviderId: "local|superadmin",
                Provider: AuthProvider.Local,
                Email: request.User.Email,
                Name: request.User.Name,
                PlainPassword: request.User.Password));

            userRepository.Add(user);

            // Create Customer via Refit
            var address = new CreateAddressApiRequest(
                request.Customer.Address.Street,
                request.Customer.Address.City,
                request.Customer.Address.PostalCode,
                request.Customer.Address.Region,
                request.Customer.Address.Country,
                request.Customer.Address.Latitude,
                request.Customer.Address.Longitude);

            await customerApi.CreateAsync(new CreateCustomerApiRequest(
                Id: platformTenantId,
                Name: request.Customer.Name,
                Slug: request.Customer.Slug,
                Description: request.Customer.Description,
                EstablishmentType: request.Customer.EstablishmentType,
                DefaultCulture: request.Customer.DefaultCulture,
                TimeZoneId: request.Customer.TimeZoneId,
                Address: address,
                ContactInfo: new CreateContactInfoApiRequest(
                    request.Customer.ContactInfo.Phone,
                    request.Customer.ContactInfo.Email,
                    request.Customer.ContactInfo.WebsiteUrl),
                BillingInfo: new CreateBillingInfoApiRequest(
                    request.Customer.BillingInfo.BusinessName,
                    request.Customer.BillingInfo.TaxId,
                    new CreateAddressApiRequest(
                        request.Customer.BillingInfo.BillingAddress.Street,
                        request.Customer.BillingInfo.BillingAddress.City,
                        request.Customer.BillingInfo.BillingAddress.PostalCode,
                        request.Customer.BillingInfo.BillingAddress.Region,
                        request.Customer.BillingInfo.BillingAddress.Country,
                        request.Customer.BillingInfo.BillingAddress.Latitude,
                        request.Customer.BillingInfo.BillingAddress.Longitude))));

            // Create Owner Role
            var ownerRole = createOwnerRole.Execute(new CreateOwnerRoleCommand(
                TenantId: platformTenantId));

            roleRepository.Add(ownerRole);

            // Create Membership + accept immediately
            var membership = createMembership.Execute(new CreateMembershipCommand(
                TenantId: platformTenantId,
                InvitationEmail: request.User.Email,
                Role: ownerRole));

            acceptInvitation.Execute(membership, new AcceptInvitationCommand(User: user));
            membershipRepository.Add(membership);

            await unitOfWork.SaveChangesAsync();
        }
    }

    [GenerateRepository<User>]
    public interface IUserRepository : IAdd<User>
    {
        Task<User?> FindFirstByEmailAndProvider(string email, AuthProvider provider);
    }

    public interface IRoleRepository : IAdd<TenantRole> { }

    public interface IMembershipRepository : IAdd<Membership> { }
}
