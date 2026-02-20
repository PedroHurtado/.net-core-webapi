namespace Auth.Features.Users.Domain.UserAggregate;

public record UpdateUserCommand(
    string Name,
    string? Phone
);

public partial class User
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<User> userValidator
    ) : AbstractModifyCommand<UpdateUserCommand, User>
    {
        public override User Execute(User user, UpdateUserCommand command)
        {
            user.Name = command.Name;
            user.Phone = command.Phone;

            return userValidator.ValidateOrThrow(user);
        }
    }
}
