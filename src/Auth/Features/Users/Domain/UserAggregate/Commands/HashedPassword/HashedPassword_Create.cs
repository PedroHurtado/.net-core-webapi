namespace Auth.Features.Users.Domain.UserAggregate.ValueObjects;

public record CreateHashedPasswordCommand(
    string PlainPassword
);

public partial record HashedPassword
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<HashedPassword> hashedPasswordValidator,
        IPasswordHasher passwordHasher
    ) : AbstractCreateCommand<CreateHashedPasswordCommand, HashedPassword>
    {
        public override HashedPassword Execute(CreateHashedPasswordCommand command)
        {
            var salt = passwordHasher.GenerateSalt();
            var hash = passwordHasher.Hash(command.PlainPassword, salt);

            var hashedPassword = new HashedPassword(hash, salt);

            return hashedPasswordValidator.ValidateOrThrow(hashedPassword);
        }
    }
}
