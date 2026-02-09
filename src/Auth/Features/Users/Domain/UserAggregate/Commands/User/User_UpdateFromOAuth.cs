namespace Auth.Features.Users.Domain.UserAggregate;

public record UpdateFromOAuthCommand(
    string Email,
    string Name,
    string? AvatarUrl
);

public partial class User
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdateFromOAuth(
        IValidator<User> userValidator,
        TimeProvider timeProvider
    ) : AbstractModifyCommand<UpdateFromOAuthCommand, User>
    {
        public override User Execute(User user, UpdateFromOAuthCommand command)
        {
            user.Email = command.Email;
            user.Name = command.Name;
            user.AvatarUrl = command.AvatarUrl;
            user.LastLoginAt = timeProvider.GetUtcNow().UtcDateTime;

            return userValidator.ValidateOrThrow(user);
        }
    }
}
