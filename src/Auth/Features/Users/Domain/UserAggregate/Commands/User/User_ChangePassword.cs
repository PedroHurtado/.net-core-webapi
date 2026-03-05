namespace Auth.Features.Users.Domain.UserAggregate;

public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword
);

public partial class User
{
    [Injectable(ServiceLifetime.Singleton)]
    public class ChangePassword(
        HashedPassword.Create createHashedPassword,
        IPasswordHasher passwordHasher,
        IValidator<User> userValidator
    ) : AbstractModifyCommand<ChangePasswordCommand, User>
    {
        public override User Execute(User user, ChangePasswordCommand command)
        {
            ConflictGuard.ThrowIf(user.Provider != AuthProvider.Local,
                "Password change is only available for local users");

            var isValid = passwordHasher.Verify(
                command.CurrentPassword, user.Password!.Hash, user.Password.Salt);

            UnauthorizedGuard.ThrowIf(!isValid, "Invalid credentials");

            user.Password = createHashedPassword.Execute(
                new CreateHashedPasswordCommand(command.NewPassword));

            return userValidator.ValidateOrThrow(user);
        }
    }
}
