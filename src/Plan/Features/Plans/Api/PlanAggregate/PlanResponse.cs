namespace Plan.Features.Plans.Api.PlanAggregate;

public record PlanResponse(
    Guid Id,
    string Name,
    string Description,
    MoneyResponse Price,
    BillingPeriod BillingPeriod,
    bool IsActive,
    bool HasActiveProvider,
    IReadOnlyCollection<FeatureResponse> Features,
    IReadOnlyCollection<ProviderConfigResponse> ProviderConfigurations)
{
    public static PlanResponse Map(PlanAgg plan) => new(
        Id: plan.Id,
        Name: plan.Name,
        Description: plan.Description,
        Price: MoneyResponse.Map(plan.Price),
        BillingPeriod: plan.BillingPeriod,
        IsActive: plan.IsActive,
        HasActiveProvider: plan.HasActiveProvider,
        Features: plan.Features
            .Select(FeatureResponse.Map)
            .ToList()
            .AsReadOnly(),
        ProviderConfigurations: plan.ProviderConfigurations
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