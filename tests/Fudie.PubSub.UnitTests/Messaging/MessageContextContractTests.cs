namespace Fudie.PubSub.UnitTests.Messaging;

public class MessageContextContractTests
{
    [Fact]
    public void IMessageContext_HasTenantId()
    {
        typeof(IMessageContext).GetProperty("TenantId").Should().NotBeNull();
        typeof(IMessageContext).GetProperty("TenantId")!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void IMessageContext_HasUserId()
    {
        typeof(IMessageContext).GetProperty("UserId").Should().NotBeNull();
        typeof(IMessageContext).GetProperty("UserId")!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void IMessageContext_HasCorrelationId()
    {
        typeof(IMessageContext).GetProperty("CorrelationId").Should().NotBeNull();
        typeof(IMessageContext).GetProperty("CorrelationId")!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void IMessageContext_HasClaims()
    {
        typeof(IMessageContext).GetProperty("Claims").Should().NotBeNull();
        typeof(IMessageContext).GetProperty("Claims")!.PropertyType.Should().Be(typeof(IDictionary<string, string>));
    }

    [Fact]
    public void IMessagePublisher_HasPublishAsyncMethod()
    {
        var method = typeof(IMessagePublisher).GetMethod("PublishAsync");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task));
        method.IsGenericMethod.Should().BeTrue();
    }
}
