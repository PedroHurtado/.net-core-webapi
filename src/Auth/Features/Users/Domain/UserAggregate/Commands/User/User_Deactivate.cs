namespace Auth.Features.Users.Domain.UserAggregate;

public record DeactivateUserCommand;

public partial class User
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Deactivate(
        IValidator<User> userValidator
    ) : AbstractModifyCommand<DeactivateUserCommand, User>
    {
        public override User Execute(User user, DeactivateUserCommand command)
        {
            ConflictGuard.ThrowIf(!user.IsActive, "User is already inactive");

            user.IsActive = false;

            return userValidator.ValidateOrThrow(user);
        }
    }
}
