namespace Auth.Features.Users.Domain.UserAggregate;

public record CreateUserCommand(
    string ProviderId,
    AuthProvider Provider,
    string Email,
    string Name,
    string? Phone = null,
    string? AvatarUrl = null,
    string? PlainPassword = null
);

public partial class User
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        HashedPassword.Create createHashedPassword,
        IValidator<User> userValidator
    ) : AbstractCreateCommand<CreateUserCommand, User>
    {
        public override User Execute(CreateUserCommand command)
        {
            var password = command.PlainPassword is not null
                ? createHashedPassword.Execute(new CreateHashedPasswordCommand(command.PlainPassword))
                : null;

            var user = new User(Guid.NewGuid())
            {
                ProviderId = command.ProviderId,
                Provider = command.Provider,
                Email = command.Email,
                Name = command.Name,
                Phone = command.Phone,
                AvatarUrl = command.AvatarUrl,
                Password = password,
                LastLoginAt = null,
                IsActive = true
            };

            return userValidator.ValidateOrThrow(user);
        }
    }
}