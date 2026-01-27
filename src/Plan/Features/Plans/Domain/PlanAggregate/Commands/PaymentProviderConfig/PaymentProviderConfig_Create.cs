namespace Plans.Features.Plans.Domain.PlanAggregate.ValueObjects;

/// <summary>
/// Command data for creating a payment provider configuration.
/// </summary>
/// <param name="Provider">The payment provider name. Must not be empty and cannot exceed 50 characters.</param>
/// <param name="ExternalProductId">The external product ID. Must not be empty and cannot exceed 100 characters.</param>
/// <param name="ExternalPriceId">The external price ID. Must not be empty and cannot exceed 100 characters.</param>
/// <param name="IsActive">Whether the configuration is active. Defaults to true.</param>
public record CreatePaymentProviderConfigCommand(
    string Provider,
    string ExternalProductId,
    string ExternalPriceId,
    bool IsActive = true
);

public partial record PaymentProviderConfig
{
    /// <summary>
    /// Command handler for creating a new <see cref="PaymentProviderConfig"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This command creates a payment provider configuration that maps the plan
    /// to external product and price IDs in the payment provider system.
    /// </para>
    /// <para>
    /// The command validates the configuration using <see cref="PaymentProviderConfigValidator"/> before returning.
    /// </para>
    /// </remarks>
    /// <param name="paymentProviderConfigValidator">The validator for payment provider config instances.</param>
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<PaymentProviderConfig> paymentProviderConfigValidator
    ) : AbstractCreateCommand<CreatePaymentProviderConfigCommand, PaymentProviderConfig>
    {
        /// <summary>
        /// Executes the create payment provider configuration command.
        /// </summary>
        /// <param name="command">The command containing the payment provider configuration creation data.</param>
        /// <returns>A new validated <see cref="PaymentProviderConfig"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the payment provider configuration data is invalid.</exception>
        public override PaymentProviderConfig Execute(CreatePaymentProviderConfigCommand command)
        {
            var config = new PaymentProviderConfig(
                command.Provider,
                command.ExternalProductId,
                command.ExternalPriceId,
                command.IsActive);

            return paymentProviderConfigValidator.ValidateOrThrow(config);
        }
    }
}
