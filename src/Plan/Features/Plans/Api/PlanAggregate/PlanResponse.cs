namespace Plans.Features.Plans.Api.PlanAggregate;

public record PlanResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    bool HasActivePricingTierWithProvider,
    IReadOnlyCollection<FeatureResponse> Features,
    IReadOnlyCollection<PricingTierResponse> PricingTiers)
{
    public static PlanResponse Map(Plan plan) => new(
        Id: plan.Id,
        Name: plan.Name,
        Description: plan.Description,
        IsActive: plan.IsActive,
        HasActivePricingTierWithProvider: plan.HasActivePricingTierWithProvider,
        Features: plan.Features
            .Select(FeatureResponse.Map)
            .ToList()
            .AsReadOnly(),
        PricingTiers: plan.PricingTiers
            .Select(PricingTierResponse.Map)
            .ToList()
            .AsReadOnly());
}

public record PricingTierResponse(
    BillingPeriod BillingPeriod,
    MoneyResponse Price,
    bool IsActive,
    bool HasActiveProvider,
    IReadOnlyCollection<ProviderConfigResponse> ProviderConfigurations)
{
    public static PricingTierResponse Map(PricingTier tier) => new(
        BillingPeriod: tier.BillingPeriod,
        Price: MoneyResponse.Map(tier.Price),
        IsActive: tier.IsActive,
        HasActiveProvider: tier.HasActiveProvider,
        ProviderConfigurations: tier.ProviderConfigurations
            .Select(ProviderConfigResponse.Map)
            .ToList()
            .AsReadOnly());
}

public record MoneyResponse(
    decimal Amount,
    CurrencyResponse Currency)
{
    public static MoneyResponse Map(Money money) => new(
        Amount: money.Amount,
        Currency: CurrencyResponse.Map(money.Currency));
}

public record CurrencyResponse(
    string Code,
    string Symbol,
    int DecimalPlaces)
{
    public static CurrencyResponse Map(Currency currency) => new(
        Code: currency.Code,
        Symbol: currency.Symbol,
        DecimalPlaces: currency.DecimalPlaces);
}

public record FeatureResponse(
    string Code,
    string Name,
    string? Description,
    FeatureType Type,
    int? Limit,
    string? Unit,
    string DisplayValue)
{
    public static FeatureResponse Map(Feature feature) => new(
        Code: feature.Code,
        Name: feature.Name,
        Description: feature.Description,
        Type: feature.Type,
        Limit: feature.Limit,
        Unit: feature.Unit,
        DisplayValue: feature.DisplayValue);
}

public record ProviderConfigResponse(
    string Provider,
    string ExternalProductId,
    string ExternalPriceId,
    bool IsActive)
{
    public static ProviderConfigResponse Map(PaymentProviderConfig config) => new(
        Provider: config.Provider,
        ExternalProductId: config.ExternalProductId,
        ExternalPriceId: config.ExternalPriceId,
        IsActive: config.IsActive);
}
