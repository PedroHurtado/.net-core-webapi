namespace Fudie.PubSub.UnitTests.Hosting;

public class MessageHandlerContractTests
{
    [Fact]
    public void IMessageHandler_HasHandleMethod()
    {
        var method = typeof(IMessageHandler<SampleMessage>).GetMethod("Handle");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task));
        method.GetParameters().Should().HaveCount(2);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(SampleMessage));
        method.GetParameters()[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact]
    public void IMessageHandler_IsContravariant()
    {
        typeof(IMessageHandler<>).GetGenericArguments()[0]
            .GenericParameterAttributes
            .Should().HaveFlag(System.Reflection.GenericParameterAttributes.Contravariant);
    }
}
