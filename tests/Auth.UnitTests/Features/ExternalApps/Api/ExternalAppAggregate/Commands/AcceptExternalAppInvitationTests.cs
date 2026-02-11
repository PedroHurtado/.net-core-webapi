namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class AcceptExternalAppInvitationTests : IClassFixture<DomainFixture>
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<AcceptExternalAppInvitation.IRepository> _repository = new();
    private readonly Mock<IEntityLookup> _entityLookup = new();
    private readonly Mock<IApiKeyGenerator> _apiKeyGenerator = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly AcceptExternalAppInvitation.Service _service;

    public AcceptExternalAppInvitationTests(DomainFixture fixture)
    {
        _service = new AcceptExternalAppInvitation.Service(
            new CurrentUserId(_userId),
            fixture.Get<ExternalApp.AcceptInvitation>(),
            _apiKeyGenerator.Object,
            _repository.Object,
            _entityLookup.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithPendingInvitation_ReturnsApiKeyResponse()
    {
        var user = new TestableUser(_userId).WithName("Dev User");
        var entity = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);
        _entityLookup.Setup(e => e.GetRequiredAsync<User, Guid>(
            It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(user);
        _apiKeyGenerator.Setup(g => g.Generate()).Returns(
            new ApiKeyResult("fud_testkey12345678901234567890ab", "hashed", "salted", "fud_test"));

        var response = await _service.HandleAsync(entity.Id);

        response.ApiKey.Should().Be("fud_testkey12345678901234567890ab");
        response.Prefix.Should().Be("fud_test");
        response.Message.Should().NotBeEmpty();
    }
}
