namespace Auth.UnitTests.Features.Users.Api.UserAggregate.Commands;

public class SeedTests : IClassFixture<DomainFixture>
{
    private readonly Mock<IConfiguration> _configuration = new();
    private readonly Mock<ICustomerApi> _customerApi = new();
    private readonly Mock<Seed.IUserRepository> _userRepository = new();
    private readonly Mock<Seed.IRoleRepository> _roleRepository = new();
    private readonly Mock<Seed.IMembershipRepository> _membershipRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Seed.Service _service;

    private readonly string _seedKey = "test-seed-key";
    private readonly Guid _platformTenantId = Guid.NewGuid();

    public SeedTests(DomainFixture fixture)
    {
        _configuration.Setup(c => c["Fudie:SeedKey"]).Returns(_seedKey);
        _configuration.Setup(c => c["Fudie:PlatformTenantId"]).Returns(_platformTenantId.ToString());

        _service = new Seed.Service(
            _configuration.Object,
            fixture.Get<User.Create>(),
            fixture.Get<TenantRole.CreateOwnerRole>(),
            fixture.Get<Membership.Create>(),
            fixture.Get<Membership.AcceptInvitation>(),
            _customerApi.Object,
            _userRepository.Object,
            _roleRepository.Object,
            _membershipRepository.Object,
            _unitOfWork.Object);
    }

    private Seed.Request CreateValidRequest() => new(
        User: new Seed.SeedUserRequest(
            Email: "admin@fudie.app",
            Name: "Fudie Admin",
            Password: "SecureP@ss123"),
        Customer: new Seed.SeedCustomerRequest(
            Name: "Fudie Platform",
            Slug: "fudie-platform",
            Description: "Tenant de plataforma",
            EstablishmentType: "platform",
            DefaultCulture: "es-ES",
            TimeZoneId: "Europe/Madrid",
            Address: new Seed.SeedAddressRequest(
                Street: "Calle Principal 1",
                City: "Madrid",
                PostalCode: "28001",
                Region: "Madrid",
                Country: "ES",
                Latitude: 40.4168m,
                Longitude: -3.7038m),
            ContactInfo: new Seed.SeedContactInfoRequest(
                Phone: "+34900000000",
                Email: "info@fudie.app",
                WebsiteUrl: "https://fudie.app"),
            BillingInfo: new Seed.SeedBillingInfoRequest(
                BusinessName: "Fudie Technologies S.L.",
                TaxId: "B12345678",
                BillingAddress: new Seed.SeedAddressRequest(
                    Street: "Calle Principal 1",
                    City: "Madrid",
                    PostalCode: "28001",
                    Region: "Madrid",
                    Country: "ES",
                    Latitude: 40.4168m,
                    Longitude: -3.7038m))));

    // ──────────────────────────────────────────────
    // Seed Key validation
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithNullSeedKey_ThrowsUnauthorized()
    {
        var act = () => _service.HandleAsync(null, CreateValidRequest());

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSeedKey_ThrowsUnauthorized()
    {
        var act = () => _service.HandleAsync("wrong-key", CreateValidRequest());

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    // ──────────────────────────────────────────────
    // Conflict guard
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WhenOwnerAlreadyExists_ThrowsConflict()
    {
        var existingUser = new TestableUser(Guid.NewGuid())
            .WithProviderId("local|superadmin")
            .WithProvider(AuthProvider.Local)
            .WithEmail("admin@fudie.app")
            .WithName("Existing Admin")
            .WithIsActive(true);

        _userRepository.Setup(r => r.FindFirstByEmailAndProvider("admin@fudie.app", AuthProvider.Local))
            .ReturnsAsync(existingUser);

        var act = () => _service.HandleAsync(_seedKey, CreateValidRequest());

        await act.Should().ThrowAsync<ConflictException>();
    }

    // ──────────────────────────────────────────────
    // Successful seed
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_AddsNewUser()
    {
        await _service.HandleAsync(_seedKey, CreateValidRequest());

        _userRepository.Verify(r => r.Add(It.Is<User>(u =>
            u.Email == "admin@fudie.app" &&
            u.Name == "Fudie Admin" &&
            u.Provider == AuthProvider.Local)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CreatesCustomerViaRefit()
    {
        await _service.HandleAsync(_seedKey, CreateValidRequest());

        _customerApi.Verify(c => c.CreateAsync(It.Is<CreateCustomerApiRequest>(r =>
            r.Name == "Fudie Platform" &&
            r.Slug == "fudie-platform")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AddsOwnerRole()
    {
        await _service.HandleAsync(_seedKey, CreateValidRequest());

        _roleRepository.Verify(r => r.Add(It.Is<TenantRole>(role =>
            role.TenantId == _platformTenantId &&
            role.Name == "Owner" &&
            role.IsOwner == true)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AddsAcceptedMembership()
    {
        await _service.HandleAsync(_seedKey, CreateValidRequest());

        _membershipRepository.Verify(r => r.Add(It.Is<Membership>(m =>
            m.TenantId == _platformTenantId &&
            m.InvitationEmail == "admin@fudie.app" &&
            m.InvitationStatus == InvitationStatus.Accepted &&
            m.User != null)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SavesChangesOnce()
    {
        await _service.HandleAsync(_seedKey, CreateValidRequest());

        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    // ──────────────────────────────────────────────
    // Handler
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handler_ExtractsSeedKeyFromHeader()
    {
        var mockService = new Mock<Seed.IService>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Seed-Key"] = "my-key";
        var request = CreateValidRequest();

        await Seed.Handler(mockService.Object, httpContext, request);

        mockService.Verify(s => s.HandleAsync("my-key", request), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNoContent()
    {
        var mockService = new Mock<Seed.IService>();
        var httpContext = new DefaultHttpContext();
        var request = CreateValidRequest();

        var result = await Seed.Handler(mockService.Object, httpContext, request);

        result.Should().BeOfType<NoContent>();
    }
}
