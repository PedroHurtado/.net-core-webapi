namespace Auth.UnitTests.Infrastructure.SessionManagement;

public class RequestTimestampTests
{
    [Fact]
    public void UtcNow_ReturnsCurrentTime()
    {
        var mockSettings = new Mock<ISessionSettings>();
        mockSettings.Setup(s => s.Get()).Returns(new SessionSettings("fudie_session", 30));

        var timestamp = new RequestTimestamp(mockSettings.Object);

        timestamp.UtcNow.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ExpiresAt_ReturnsUtcNowPlusExpirationDays()
    {
        var mockSettings = new Mock<ISessionSettings>();
        mockSettings.Setup(s => s.Get()).Returns(new SessionSettings("fudie_session", 30));

        var timestamp = new RequestTimestamp(mockSettings.Object);

        timestamp.ExpiresAt.Should().BeCloseTo(timestamp.UtcNow.AddDays(30), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ExpiresAt_UsesConfiguredExpirationDays()
    {
        var mockSettings = new Mock<ISessionSettings>();
        mockSettings.Setup(s => s.Get()).Returns(new SessionSettings("fudie_session", 7));

        var timestamp = new RequestTimestamp(mockSettings.Object);

        timestamp.ExpiresAt.Should().BeCloseTo(timestamp.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));
    }
}
