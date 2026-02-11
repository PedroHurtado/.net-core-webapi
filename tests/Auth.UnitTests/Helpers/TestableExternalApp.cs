namespace Auth.UnitTests.Helpers;

public class TestableExternalApp : ExternalApp
{
    public TestableExternalApp(Guid id) : base(id) { }

    public TestableExternalApp WithTenantId(Guid value) { TenantId = value; return this; }
    public TestableExternalApp WithUser(User? value) { User = value; return this; }
    public TestableExternalApp WithName(string value) { Name = value; return this; }
    public TestableExternalApp WithIsActive(bool value) { IsActive = value; return this; }
    public TestableExternalApp WithInvitationEmail(string value) { InvitationEmail = value; return this; }
    public TestableExternalApp WithInvitationStatus(InvitationStatus value) { InvitationStatus = value; return this; }
    public TestableExternalApp WithApiKeyHash(string? value) { ApiKeyHash = value; return this; }
    public TestableExternalApp WithApiKeySalt(string? value) { ApiKeySalt = value; return this; }
    public TestableExternalApp WithApiKeyPrefix(string? value) { ApiKeyPrefix = value; return this; }
    public TestableExternalApp WithApiKeyExpiresAt(DateTime? value) { ApiKeyExpiresAt = value; return this; }
    public TestableExternalApp WithGroup(string group) { _groups.Add(group); return this; }
    public TestableExternalApp WithAdditionalScope(string scope) { _additionalScopes.Add(scope); return this; }
    public TestableExternalApp WithExcludedScope(string scope) { _excludedScopes.Add(scope); return this; }
}
