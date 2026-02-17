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
    public void GetAsync_HasGetAttribute()
    {
        var method = typeof(ICustomerApi).GetMethod(nameof(ICustomerApi.GetAsync));

        var attr = method!.GetCustomAttributes(typeof(GetAttribute), false);

        attr.Should().ContainSingle();
        ((GetAttribute)attr[0]).Path.Should().Be("/customer");
    }

    [Fact]
    public void UpdateAsync_HasPutAttribute()
    {
        var method = typeof(ICustomerApi).GetMethod(nameof(ICustomerApi.UpdateAsync));

        var attr = method!.GetCustomAttributes(typeof(PutAttribute), false);

        attr.Should().ContainSingle();
        ((PutAttribute)attr[0]).Path.Should().Be("/customer");
    }

    [Fact]
    public void CreateAsync_RequestParameterHasBodyAttribute()
    {
        var method = typeof(ICustomerApi).GetMethod(nameof(ICustomerApi.CreateAsync));
        var param = method!.GetParameters()[0];

        param.GetCustomAttributes(typeof(BodyAttribute), false).Should().ContainSingle();
    }

    [Fact]
    public void UpdateAsync_RequestParameterHasBodyAttribute()
    {
        var method = typeof(ICustomerApi).GetMethod(nameof(ICustomerApi.UpdateAsync));
        var param = method!.GetParameters()[0];

        param.GetCustomAttributes(typeof(BodyAttribute), false).Should().ContainSingle();
    }

    [Fact]
    public void CreateAsync_ReturnsCreateCustomerApiResponse()
    {
        var method = typeof(ICustomerApi).GetMethod(nameof(ICustomerApi.CreateAsync));

        method!.ReturnType.Should().Be(typeof(Task<CreateCustomerApiResponse>));
    }

    [Fact]
    public void GetAsync_ReturnsCustomerApiResponse()
    {
        var method = typeof(ICustomerApi).GetMethod(nameof(ICustomerApi.GetAsync));

        method!.ReturnType.Should().Be(typeof(Task<CustomerApiResponse>));
    }

    [Fact]
    public void UpdateAsync_ReturnsTask()
    {
        var method = typeof(ICustomerApi).GetMethod(nameof(ICustomerApi.UpdateAsync));

        method!.ReturnType.Should().Be(typeof(Task));
    }
}
