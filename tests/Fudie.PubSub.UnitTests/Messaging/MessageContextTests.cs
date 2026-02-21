namespace Fudie.PubSub.UnitTests.Messaging;

public class MessageContextTests
{
    [Fact]
    public void Populate_ExtractsTenantId()
    {
        var context = new MessageContext();
        var claims = new Dictionary<string, string> { ["tenant_id"] = "tenant-42" };

        context.Populate(claims, "corr-1");

        context.TenantId.Should().Be("tenant-42");
    }

    [Fact]
    public void Populate_ExtractsUserId()
    {
        var context = new MessageContext();
        var claims = new Dictionary<string, string> { ["sub"] = "user-7" };

        context.Populate(claims, "corr-1");

        context.UserId.Should().Be("user-7");
    }

    [Fact]
    public void Populate_SetsCorrelationId()
    {
        var context = new MessageContext();

        context.Populate(null, "corr-99");

        context.CorrelationId.Should().Be("corr-99");
    }

    [Fact]
    public void Populate_CopiesClaims()
    {
        var context = new MessageContext();
        var claims = new Dictionary<string, string>
        {
            ["sub"] = "user-1",
            ["tenant_id"] = "t-1",
            ["role"] = "admin"
        };

        context.Populate(claims, null);

        context.Claims.Should().HaveCount(3);
        context.Claims["role"].Should().Be("admin");
    }

    [Fact]
    public void Populate_WithNullClaims_LeavesDefaults()
    {
        var context = new MessageContext();

        context.Populate(null, null);

        context.TenantId.Should().BeNull();
        context.UserId.Should().BeNull();
        context.CorrelationId.Should().BeNull();
        context.Claims.Should().BeEmpty();
    }

    [Fact]
    public void DefaultState_IsEmpty()
    {
        var context = new MessageContext();

        context.TenantId.Should().BeNull();
        context.UserId.Should().BeNull();
        context.CorrelationId.Should().BeNull();
        context.Claims.Should().BeEmpty();
    }

    [Fact]
    public void Populate_ImplementsIMessageContext()
    {
        IMessageContext context = new MessageContext();

        context.Should().BeAssignableTo<IMessageContext>();
    }
}
