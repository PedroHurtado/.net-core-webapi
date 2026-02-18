namespace Auth.Features.Users.Domain.UserAggregate;

public record RecordLoginCommand(
    DateTime Now
);

public partial class User
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RecordLogin(
        IValidator<User> userValidator
    ) : AbstractModifyCommand<RecordLoginCommand, User>
    {
        public override User Execute(User user, RecordLoginCommand command)
        {
            user.LastLoginAt = command.Now;

            return userValidator.ValidateOrThrow(user);
        }
    }
}
