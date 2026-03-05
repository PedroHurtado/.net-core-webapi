namespace Auth.Features.Users.Api.UserAggregate;

public record UserResponse(
    Guid Id,
    string ProviderId,
    AuthProvider Provider,
    string Email,
    string Name,
    string? Phone,
    string? AvatarUrl,
    DateTime? LastLoginAt,
    bool IsActive)
{
    public static UserResponse Map(User user) => new(
        user.Id,
        user.ProviderId,
        user.Provider,
        user.Email,
        user.Name,
        user.Phone,
        user.AvatarUrl,
        user.LastLoginAt,
        user.IsActive);
}
