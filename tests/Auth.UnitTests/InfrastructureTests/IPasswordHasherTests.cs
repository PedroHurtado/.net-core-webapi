namespace Auth.UnitTests.InfrastructureTests;

public class IPasswordHasherTests
{
    [Fact]
    public void Contract_HasGenerateSaltMethod()
    {
        var method = typeof(IPasswordHasher).GetMethod("GenerateSalt");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(string));
        method.GetParameters().Should().BeEmpty();
    }

    [Fact]
    public void Contract_HasHashMethod()
    {
        var method = typeof(IPasswordHasher).GetMethod("Hash");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(string));
        method.GetParameters().Should().HaveCount(2);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(string));
        method.GetParameters()[1].ParameterType.Should().Be(typeof(string));
    }

    [Fact]
    public void Contract_HasVerifyMethod()
    {
        var method = typeof(IPasswordHasher).GetMethod("Verify");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(bool));
        method.GetParameters().Should().HaveCount(3);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(string));
        method.GetParameters()[1].ParameterType.Should().Be(typeof(string));
        method.GetParameters()[2].ParameterType.Should().Be(typeof(string));
    }

    [Fact]
    public void Contract_HasExpectedMethodCount()
    {
        var methods = typeof(IPasswordHasher).GetMethods();

        methods.Should().HaveCount(3);
    }
}
