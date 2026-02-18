namespace Auth.UnitTests.Infrastructure.SessionManagement;

public class SessionCookieServiceTests
{
    [Fact]
    public void Append_UsesConfiguredCookieName()
    {
        var now = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var mockTimestamp = new Mock<IRequestTimestamp>();
        mockTimestamp.Setup(t => t.ExpiresAt).Returns(now.AddDays(30));
        var mockSettings = new Mock<ISessionSettings>();
        mockSettings.Setup(s => s.Get()).Returns(new SessionSettings("custom_cookie", 30));

        var service = new SessionCookieService(mockTimestamp.Object, mockSettings.Object);
        var httpContext = new DefaultHttpContext();
        var sessionId = Guid.NewGuid();

        service.Append(httpContext, sessionId);

        httpContext.Response.Headers.SetCookie.ToString()
            .Should().Contain($"custom_cookie={sessionId}");
    }

    [Fact]
    public void Append_SetsExpiresFromRequestTimestamp()
    {
        var now = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var expiresAt = now.AddDays(30);
        var mockTimestamp = new Mock<IRequestTimestamp>();
        mockTimestamp.Setup(t => t.ExpiresAt).Returns(expiresAt);
        var mockSettings = new Mock<ISessionSettings>();
        mockSettings.Setup(s => s.Get()).Returns(new SessionSettings("fudie_session", 30));

        var service = new SessionCookieService(mockTimestamp.Object, mockSettings.Object);
        var httpContext = new DefaultHttpContext();

        service.Append(httpContext, Guid.NewGuid());

        httpContext.Response.Headers.SetCookie.ToString()
            .Should().Contain("expires=");
    }

    [Fact]
    public void Append_SetsPathToRoot()
    {
        var now = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var mockTimestamp = new Mock<IRequestTimestamp>();
        mockTimestamp.Setup(t => t.ExpiresAt).Returns(now.AddDays(30));
        var mockSettings = new Mock<ISessionSettings>();
        mockSettings.Setup(s => s.Get()).Returns(new SessionSettings("fudie_session", 30));

        var service = new SessionCookieService(mockTimestamp.Object, mockSettings.Object);
        var httpContext = new DefaultHttpContext();

        service.Append(httpContext, Guid.NewGuid());

        httpContext.Response.Headers.SetCookie.ToString()
            .Should().Contain("path=/");
    }

    [Fact]
    public void Append_SetsSameSiteLax()
    {
        var now = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var mockTimestamp = new Mock<IRequestTimestamp>();
        mockTimestamp.Setup(t => t.ExpiresAt).Returns(now.AddDays(30));
        var mockSettings = new Mock<ISessionSettings>();
        mockSettings.Setup(s => s.Get()).Returns(new SessionSettings("fudie_session", 30));

        var service = new SessionCookieService(mockTimestamp.Object, mockSettings.Object);
        var httpContext = new DefaultHttpContext();

        service.Append(httpContext, Guid.NewGuid());

        httpContext.Response.Headers.SetCookie.ToString()
            .Should().ContainEquivalentOf("samesite=lax");
    }
}
