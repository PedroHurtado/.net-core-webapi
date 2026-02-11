namespace Auth.UnitTests.Helpers;

public class TestableMembership : Membership
{
    public TestableMembership(Guid id) : base(id) { }

    public TestableMembership WithTenantId(Guid value) { TenantId = value; return this; }
    public TestableMembership WithUser(User? value) { User = value; return this; }
    public TestableMembership WithRole(TenantRole value) { Role = value; return this; }
    public TestableMembership WithIsActive(bool value) { IsActive = value; return this; }
    public TestableMembership WithInvitationEmail(string value) { InvitationEmail = value; return this; }
    public TestableMembership WithInvitationStatus(InvitationStatus value) { InvitationStatus = value; return this; }
}
