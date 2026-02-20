namespace Auth.UnitTests.Helpers;

public class TestableTenantRole : TenantRole
{
    public TestableTenantRole(Guid id) : base(id) { }

    public TestableTenantRole WithTenantId(Guid value) { TenantId = value; return this; }
    public TestableTenantRole WithName(string value) { Name = value; return this; }
    public TestableTenantRole WithDescription(string value) { Description = value; return this; }
    public TestableTenantRole WithIsOwner(bool value) { IsOwner = value; return this; }
    public TestableTenantRole WithGroup(string item) { _groups.Add(item); return this; }
    public TestableTenantRole WithAdditionalScope(string item) { _additionalScopes.Add(item); return this; }
    public TestableTenantRole WithExcludedScope(string item) { _excludedScopes.Add(item); return this; }
}
