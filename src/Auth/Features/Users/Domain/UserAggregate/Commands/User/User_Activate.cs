namespace Auth.Features.Users.Domain.UserAggregate;

public record ActivateUserCommand;

public partial class User
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(
        IValidator<User> userValidator
    ) : AbstractModifyCommand<ActivateUserCommand, User>
    {
        public override User Execute(User user, ActivateUserCommand command)
        {
            ConflictGuard.ThrowIf(user.IsActive, "User is already active");

            user.IsActive = true;

            return userValidator.ValidateOrThrow(user);
        }
    }
}
