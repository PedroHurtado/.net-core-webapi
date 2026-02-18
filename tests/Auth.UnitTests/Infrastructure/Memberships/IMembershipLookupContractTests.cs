namespace Auth.UnitTests.Infrastructure.Memberships;

public class IMembershipLookupContractTests
{
    [Fact]
    public void Contract_HasFindFirstByUserIdMethod()
    {
        var method = typeof(IMembershipLookup).GetMethod("FindFirstByUserId");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<Membership?>));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(Guid));
    }

    [Fact]
    public void Contract_HasFindAllByUserIdMethod()
    {
        var method = typeof(IMembershipLookup).GetMethod("FindAllByUserId");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<List<Membership>>));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(Guid));
    }

    [Fact]
    public void Contract_HasExpectedMethodCount()
    {
        var methods = typeof(IMembershipLookup).GetMethods();

        methods.Should().HaveCount(2);
    }
}
