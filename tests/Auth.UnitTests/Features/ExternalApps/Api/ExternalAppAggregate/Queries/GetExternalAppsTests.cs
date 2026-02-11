namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate.Queries;

public class GetExternalAppsTests
{
    private readonly Mock<IQuery> _query = new();
    private readonly GetExternalApps.Service _service;

    public GetExternalAppsTests()
    {
        _service = new GetExternalApps.Service(_query.Object);
    }

    [Fact]
    public async Task HandleAsync_WithEntities_ReturnsOrderedList()
    {
        var entity1 = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("Zeta App")
            .WithInvitationEmail("zeta@app.com");

        var entity2 = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("Alpha App")
            .WithInvitationEmail("alpha@app.com");

        _query.Setup(q => q.Query<ExternalApp>())
            .Returns(AsyncQueryable.Of(entity1, entity2));

        var response = await _service.HandleAsync();

        response.Should().HaveCount(2);
        response[0].Name.Should().Be("Alpha App");
        response[1].Name.Should().Be("Zeta App");
    }

    [Fact]
    public async Task HandleAsync_WithNoEntities_ReturnsEmptyList()
    {
        _query.Setup(q => q.Query<ExternalApp>())
            .Returns(AsyncQueryable.Empty<ExternalApp>());

        var response = await _service.HandleAsync();

        response.Should().BeEmpty();
    }
}
