namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class RotateExternalAppApiKeyTests : IClassFixture<DomainFixture>
{
    private readonly Mock<RotateExternalAppApiKey.IRepository> _repository = new();
    private readonly Mock<IApiKeyGenerator> _apiKeyGenerator = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RotateExternalAppApiKey.Service _service;

    public RotateExternalAppApiKeyTests(DomainFixture fixture)
    {
        _service = new RotateExternalAppApiKey.Service(
            fixture.Get<ExternalApp.RotateApiKey>(),
            _apiKeyGenerator.Object,
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithActiveAcceptedApp_ReturnsApiKeyResponse()
    {
        var entity = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(true)
            .WithApiKeyHash("old-hash")
            .WithApiKeySalt("old-salt")
            .WithApiKeyPrefix("fud_old1");

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);
        _apiKeyGenerator.Setup(g => g.Generate()).Returns(
            new ApiKeyResult("fud_newkey12345678901234567890ab", "new-hash", "new-salt", "fud_newk"));

        var response = await _service.HandleAsync(entity.Id);

        response.ApiKey.Should().Be("fud_newkey12345678901234567890ab");
        response.Prefix.Should().Be("fud_newk");
        response.Message.Should().NotBeEmpty();
    }
}
