namespace Auth.Features.Users.Domain.UserAggregate;

public partial class User
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RecordLogin(
        IValidator<User> userValidator
    ) : AbstractModifyCommand<User>
    {
        public override User Execute(User user)
        {
            user.LastLoginAt = DateTime.UtcNow;

            return userValidator.ValidateOrThrow(user);
        }
    }
}
