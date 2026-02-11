namespace Auth.Features.Memberships.Api.MembershipAggregate;

public record MembershipResponse(
    Guid Id,
    Guid? UserId,
    Guid RoleId,
    bool IsActive,
    string InvitationEmail,
    string InvitationStatus)
{
    public static MembershipResponse Map(Membership entity) => new(
        Id: entity.Id,
        UserId: entity.User?.Id,
        RoleId: entity.Role.Id,
        IsActive: entity.IsActive,
        InvitationEmail: entity.InvitationEmail,
        InvitationStatus: entity.InvitationStatus.ToString());
}
