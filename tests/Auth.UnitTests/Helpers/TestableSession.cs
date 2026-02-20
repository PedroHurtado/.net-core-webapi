namespace Auth.UnitTests.Helpers;

public class TestableSession : Session
{
    public TestableSession(Guid id) : base(id) { }

    public TestableSession WithUserId(Guid value) { UserId = value; return this; }

    public TestableSession WithTenantId(Guid? value) { TenantId = value; return this; }

    public TestableSession WithRoleId(Guid? value) { RoleId = value; return this; }

    public TestableSession WithGroups(string[] value) { Groups = value; return this; }

    public TestableSession WithAdditionalScopes(string[] value) { AdditionalScopes = value; return this; }

    public TestableSession WithExcludedScopes(string[] value) { ExcludedScopes = value; return this; }

    public TestableSession WithIsOwner(bool value) { IsOwner = value; return this; }

    public TestableSession WithCreatedAt(DateTime value) { CreatedAt = value; return this; }

    public TestableSession WithLastActivityAt(DateTime value) { LastActivityAt = value; return this; }

    public TestableSession WithExpiresAt(DateTime value) { ExpiresAt = value; return this; }
}