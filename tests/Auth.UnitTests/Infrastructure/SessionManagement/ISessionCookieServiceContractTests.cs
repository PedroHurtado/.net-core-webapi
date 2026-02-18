namespace Auth.UnitTests.Infrastructure.SessionManagement;

public abstract class ISessionCookieServiceContractTests
{
    protected abstract ISessionCookieService CreateService();

    [Fact]
    public void Append_SetsSessionCookie()
    {
        var service = CreateService();
        var httpContext = new DefaultHttpContext();
        var sessionId = Guid.NewGuid();

        service.Append(httpContext, sessionId);

        httpContext.Response.Headers.SetCookie.ToString()
            .Should().Contain(sessionId.ToString());
    }

    [Fact]
    public void Append_SetsHttpOnlyCookie()
    {
        var service = CreateService();
        var httpContext = new DefaultHttpContext();

        service.Append(httpContext, Guid.NewGuid());

        httpContext.Response.Headers.SetCookie.ToString()
            .Should().Contain("httponly");
    }

    [Fact]
    public void Append_SetsSecureCookie()
    {
        var service = CreateService();
        var httpContext = new DefaultHttpContext();

        service.Append(httpContext, Guid.NewGuid());

        httpContext.Response.Headers.SetCookie.ToString()
            .Should().Contain("secure");
    }
}

public class SessionCookieService_ContractTests : ISessionCookieServiceContractTests
{
    protected override ISessionCookieService CreateService()
    {
        var now = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var mockTimestamp = new Mock<IRequestTimestamp>();
        mockTimestamp.Setup(t => t.UtcNow).Returns(now);
        mockTimestamp.Setup(t => t.ExpiresAt).Returns(now.AddDays(30));

        var mockSettings = new Mock<ISessionSettings>();
        mockSettings.Setup(s => s.Get()).Returns(new SessionSettings("fudie_session", 30));

        return new SessionCookieService(mockTimestamp.Object, mockSettings.Object);
    }
}
