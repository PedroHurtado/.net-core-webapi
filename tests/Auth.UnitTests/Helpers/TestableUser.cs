namespace Auth.UnitTests.Helpers;

public class TestableUser : User
{
    public TestableUser(Guid id) : base(id) { }

    public TestableUser WithProviderId(string value) { ProviderId = value; return this; }

    public TestableUser WithProvider(AuthProvider value) { Provider = value; return this; }

    public TestableUser WithEmail(string value) { Email = value; return this; }

    public TestableUser WithName(string value) { Name = value; return this; }

    public TestableUser WithPhone(string? value) { Phone = value; return this; }

    public TestableUser WithAvatarUrl(string? value) { AvatarUrl = value; return this; }

    public TestableUser WithPassword(HashedPassword? value) { Password = value; return this; }

    public TestableUser WithLastLoginAt(DateTime? value) { LastLoginAt = value; return this; }

    public TestableUser WithIsActive(bool value) { IsActive = value; return this; }
}
