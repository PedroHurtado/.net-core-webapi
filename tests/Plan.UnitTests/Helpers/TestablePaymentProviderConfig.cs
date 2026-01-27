namespace Plans.UnitTests.Helpers;

public record TestablePaymentProviderConfig : PaymentProviderConfig
{
    public TestablePaymentProviderConfig(string provider, string externalProductId, string externalPriceId, bool isActive = true)
        : base(provider, externalProductId, externalPriceId, isActive) { }
}
