namespace Auth.UnitTests.Infrastructure.SessionManagement;

public abstract class IRequestTimestampContractTests
{
    protected abstract IRequestTimestamp CreateTimestamp();

    [Fact]
    public void UtcNow_ReturnsUtcDateTime()
    {
        var timestamp = CreateTimestamp();

        timestamp.UtcNow.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void ExpiresAt_IsAfterUtcNow()
    {
        var timestamp = CreateTimestamp();

        timestamp.ExpiresAt.Should().BeAfter(timestamp.UtcNow);
    }

    [Fact]
    public void UtcNow_ReturnsSameValueOnMultipleCalls()
    {
        var timestamp = CreateTimestamp();

        var first = timestamp.UtcNow;
        var second = timestamp.UtcNow;

        first.Should().Be(second);
    }
}

public class RequestTimestamp_ContractTests : IRequestTimestampContractTests
{
    protected override IRequestTimestamp CreateTimestamp()
    {
        var mockSettings = new Mock<ISessionSettings>();
        mockSettings.Setup(s => s.Get()).Returns(new SessionSettings("fudie_session", 30));
        return new RequestTimestamp(mockSettings.Object);
    }
}
