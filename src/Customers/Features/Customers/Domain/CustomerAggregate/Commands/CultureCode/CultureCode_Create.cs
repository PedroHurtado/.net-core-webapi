namespace Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;

public record CreateCultureCodeCommand(
    string Code
);

public partial record CultureCode
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<CultureCode> cultureCodeValidator
    ) : AbstractCreateCommand<CreateCultureCodeCommand, CultureCode>
    {
        public override CultureCode Execute(CreateCultureCodeCommand command)
        {
            var cultureCode = new CultureCode(
                command.Code);

            return cultureCodeValidator.ValidateOrThrow(cultureCode);
        }
    }
}
