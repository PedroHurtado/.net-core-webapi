namespace Auth.Features.Memberships.Api.MembershipAggregate;

public record MembershipResponse(
    Guid Id,
    Guid? UserId,
    string? UserName,
    string? UserPhone,
    Guid RoleId,
    string RoleName,
    bool IsActive,
    string InvitationEmail,
    string InvitationStatus)
{
    public static MembershipResponse Map(Membership entity) => new(
        Id: entity.Id,
        UserId: entity.User?.Id,
        UserName: entity.User?.Name,
        UserPhone: entity.User?.Phone,
        RoleId: entity.Role.Id,
        RoleName: entity.Role.Name,
        IsActive: entity.IsActive,
        InvitationEmail: entity.InvitationEmail,
        InvitationStatus: entity.InvitationStatus.ToString());
}
