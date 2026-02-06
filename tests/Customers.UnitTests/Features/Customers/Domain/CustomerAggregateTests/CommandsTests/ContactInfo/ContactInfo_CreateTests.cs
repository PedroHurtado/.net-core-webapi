namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class ContactInfoCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ContactInfo.Create _create = fixture.Get<ContactInfo.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsContactInfo()
    {
        var command = new CreateContactInfoCommand("639079481", "juanjo@example.com", "https://facebook.com/elbardeljuanjo");

        var result = _create.Execute(command);

        result.Phone.Should().Be("639079481");
        result.Email.Should().Be("juanjo@example.com");
        result.WebsiteUrl.Should().Be("https://facebook.com/elbardeljuanjo");
    }

    [Fact]
    public void Execute_WithPhoneOnly_ReturnsContactInfo()
    {
        var command = new CreateContactInfoCommand("639079481", null, null);

        var result = _create.Execute(command);

        result.Phone.Should().Be("639079481");
        result.Email.Should().BeNull();
        result.WebsiteUrl.Should().BeNull();
    }

    [Fact]
    public void Execute_WithEmptyPhone_ThrowsValidationException()
    {
        var command = new CreateContactInfoCommand("", "juanjo@example.com", null);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ContactInfoValidationMessages.PhoneRequired}*");
    }

    [Fact]
    public void Execute_WithInvalidEmail_ThrowsValidationException()
    {
        var command = new CreateContactInfoCommand("639079481", "not-an-email", null);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ContactInfoValidationMessages.EmailFormat}*");
    }

    [Fact]
    public void Execute_WithInvalidWebsiteUrl_ThrowsValidationException()
    {
        var command = new CreateContactInfoCommand("639079481", null, "not-a-url");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ContactInfoValidationMessages.WebsiteUrlFormat}*");
    }
}
