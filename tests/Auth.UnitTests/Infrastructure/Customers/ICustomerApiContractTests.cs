namespace Auth.UnitTests.Infrastructure.Customers;

public class ICustomerApiContractTests
{
    [Fact]
    public void CreateAsync_HasPostAttribute()
    {
        var method = typeof(ICustomerApi).GetMethod(nameof(ICustomerApi.CreateAsync));

        var attr = method!.GetCustomAttributes(typeof(PostAttribute), false);

        attr.Should().ContainSingle();
        ((PostAttribute)attr[0]).Path.Should().Be("/customer");
    }

    [Fact]
    public void CreateAsync_RequestParameterHasBodyAttribute()
    {
        var method = typeof(ICustomerApi).GetMethod(nameof(ICustomerApi.CreateAsync));
        var param = method!.GetParameters()[0];

        param.GetCustomAttributes(typeof(BodyAttribute), false).Should().ContainSingle();
    }

    [Fact]
    public void CreateAsync_ReturnsCreateCustomerApiResponse()
    {
        var method = typeof(ICustomerApi).GetMethod(nameof(ICustomerApi.CreateAsync));

        method!.ReturnType.Should().Be(typeof(Task<CreateCustomerApiResponse>));
    }
}
