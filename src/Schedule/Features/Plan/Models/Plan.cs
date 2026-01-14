using Fudie.Domain;
using FluentValidation;

namespace Schedule.Features.Plan.Models;

public enum BillingPeriod
{
    Monthly,    // Mensual
    Quarterly,  // Trimestral (3 meses)
    Semester,   // Semestral (6 meses)
    Yearly      // Anual
}

public enum FeatureType
{
    Boolean,    // Feature activo o no (ej: "Soporte prioritario")
    Limit,      // Feature con límite numérico (ej: 100 reservas/mes)
    Unlimited   // Feature sin límite (ej: reservas ilimitadas)
}

public record Currency(string Code, string Symbol, int DecimalPlaces = 2)
{
    public static Currency EUR => new("EUR", "€", 2);
    public static Currency USD => new("USD", "$", 2);
    public static Currency GBP => new("GBP", "£", 2);

    public static Result<Currency> FromCode(string code)
    {
        return code.ToUpper() switch
        {
            "EUR" => Result<Currency>.Success(EUR),
            "USD" => Result<Currency>.Success(USD),
            "GBP" => Result<Currency>.Success(GBP),
            _ => Result<Currency>.Failure($"Currency {code} not supported", "Currency.Code")
        };
    }
}

public record Money(decimal Amount, Currency Currency)
{
    public static Money Zero(Currency currency) => new(0, currency);

    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
            return Result<Money>.Failure("Cannot add money with different currencies", "Money.Currency");
        return Result<Money>.Success(new Money(Amount + other.Amount, Currency));
    }
}

public record Feature(
    string Code,
    string Name,
    string? Description,
    FeatureType Type,
    int? Limit = null,
    string? Unit = null
)
{
    public bool IsValid => Type switch
    {
        FeatureType.Limit => Limit.HasValue && Limit > 0,
        FeatureType.Boolean => !Limit.HasValue,
        FeatureType.Unlimited => !Limit.HasValue,
        _ => false
    };

    public string DisplayValue => Type switch
    {
        FeatureType.Limit => $"{Limit} {Unit}",
        FeatureType.Unlimited => "Ilimitado",
        FeatureType.Boolean => "Incluido",
        _ => ""
    };
}

public record PaymentProviderConfig(
    string Provider,
    string ExternalProductId,
    string ExternalPriceId,
    bool IsActive = true
);

public class Plan : Entity<Guid>
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public Money Price { get; protected set; }
    public BillingPeriod BillingPeriod { get; protected set; }
    public bool IsActive { get; protected set; }

    public IReadOnlyCollection<Feature> Features => _features.ToList().AsReadOnly();
    protected List<Feature> _features = [];

    public IReadOnlyCollection<PaymentProviderConfig> ProviderConfigurations => _providerConfigurations.ToList().AsReadOnly();
    protected List<PaymentProviderConfig> _providerConfigurations = [];

    protected Plan(
        Guid id,
        string name,
        string description,
        Money price,
        BillingPeriod billingPeriod,
        bool isActive) : base(id)
    {
        Name = name;
        Description = description;
        Price = price;
        BillingPeriod = billingPeriod;
        IsActive = isActive;
    }

    public static Result<Plan> Create(
        Guid id,
        string name,
        string description,
        Money price,
        BillingPeriod billingPeriod,
        IEnumerable<Feature> features,
        IEnumerable<PaymentProviderConfig> providerConfigurations,
        bool isActive = true)
    {
        var plan = new Plan(id, name, description, price, billingPeriod, isActive);

        foreach (var feature in features)
        {
            var addResult = plan.AddFeature(feature);
            if (addResult.IsFailure)
                return Result<Plan>.Failure(addResult.Errors);
        }

        foreach (var config in providerConfigurations)
        {
            var addResult = plan.AddProviderConfiguration(config);
            if (addResult.IsFailure)
                return Result<Plan>.Failure(addResult.Errors);
        }

        var validation = plan.ValidateEntity(plan, new PlanValidator());
        return validation.IsFailure ? Result<Plan>.Failure(validation.Errors) : Result<Plan>.Success(plan);
    }

    // Métodos de Gestión de Features
    public Result AddFeature(Feature feature)
    {
        if (!feature.IsValid)
        {
            return Result.Failure(
                feature.Type == FeatureType.Limit
                    ? "Feature de tipo Limit requiere un valor"
                    : "Feature de tipo Boolean o Unlimited no debe tener límite",
                "Feature");
        }

        if (_features.Any(f => f.Code.Equals(feature.Code, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure($"Ya existe un Feature con código {feature.Code}", "Feature.Code");
        }

        var normalizedFeature = feature with { Code = feature.Code.ToUpper().Replace(" ", "_") };
        _features.Add(normalizedFeature);
        return Result.Success();
    }

    public Result UpdateFeature(string code, Feature updatedFeature)
    {
        var existingFeature = _features.FirstOrDefault(f => f.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (existingFeature == null)
        {
            return Result.Failure("Feature no encontrado", "Feature.Code");
        }

        if (!updatedFeature.IsValid)
        {
            return Result.Failure("Feature inválido", "Feature");
        }

        if (!updatedFeature.Code.Equals(code, StringComparison.OrdinalIgnoreCase) &&
            _features.Any(f => f.Code.Equals(updatedFeature.Code, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure($"Ya existe un Feature con código {updatedFeature.Code}", "Feature.Code");
        }

        _features.Remove(existingFeature);
        var normalizedFeature = updatedFeature with { Code = updatedFeature.Code.ToUpper().Replace(" ", "_") };
        _features.Add(normalizedFeature);
        return Result.Success();
    }

    public Result RemoveFeature(string code)
    {
        if (_features.Count <= 1)
        {
            return Result.Failure("El plan debe tener al menos una característica", "Features");
        }

        var feature = _features.FirstOrDefault(f => f.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (feature == null)
        {
            return Result.Failure("Feature no encontrado", "Feature.Code");
        }

        _features.Remove(feature);
        return Result.Success();
    }

    public Feature? GetFeatureByCode(string code)
    {
        return _features.FirstOrDefault(f => f.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    public int? GetLimitForFeature(string code)
    {
        var feature = GetFeatureByCode(code);
        if (feature == null) return null;

        return feature.Type switch
        {
            FeatureType.Limit => feature.Limit,
            FeatureType.Unlimited => null,
            _ => null
        };
    }

    public bool HasFeature(string code)
    {
        return _features.Any(f => f.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsFeatureUnlimited(string code)
    {
        var feature = GetFeatureByCode(code);
        return feature?.Type == FeatureType.Unlimited;
    }

    // Métodos de Gestión de Configuraciones de Proveedor
    public Result AddProviderConfiguration(PaymentProviderConfig config)
    {
        if (_providerConfigurations.Any(p =>
            p.Provider.Equals(config.Provider, StringComparison.OrdinalIgnoreCase) &&
            p.IsActive &&
            config.IsActive))
        {
            return Result.Failure($"Ya existe una configuración activa para {config.Provider}", "ProviderConfiguration.Provider");
        }

        _providerConfigurations.Add(config);
        return Result.Success();
    }

    public Result UpdateProviderConfiguration(string provider, string newProductId, string newPriceId)
    {
        var config = _providerConfigurations.FirstOrDefault(p =>
            p.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase));

        if (config == null)
        {
            return Result.Failure("Configuración de proveedor no encontrada", "ProviderConfiguration.Provider");
        }

        _providerConfigurations.Remove(config);
        var updatedConfig = config with
        {
            ExternalProductId = newProductId,
            ExternalPriceId = newPriceId
        };
        _providerConfigurations.Add(updatedConfig);
        return Result.Success();
    }

    public Result DeactivateProviderConfiguration(string provider)
    {
        if (_providerConfigurations.Count(p => p.IsActive) <= 1)
        {
            return Result.Failure("El plan debe tener al menos una configuración de proveedor activa", "ProviderConfigurations");
        }

        var config = _providerConfigurations.FirstOrDefault(p =>
            p.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase) && p.IsActive);

        if (config == null)
        {
            return Result.Failure("Configuración de proveedor activa no encontrada", "ProviderConfiguration.Provider");
        }

        _providerConfigurations.Remove(config);
        _providerConfigurations.Add(config with { IsActive = false });
        return Result.Success();
    }

    public Result ActivateProviderConfiguration(string provider)
    {
        var config = _providerConfigurations.FirstOrDefault(p =>
            p.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase) && !p.IsActive);

        if (config == null)
        {
            return Result.Failure("Configuración de proveedor inactiva no encontrada", "ProviderConfiguration.Provider");
        }

        if (_providerConfigurations.Any(p =>
            p.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase) && p.IsActive))
        {
            return Result.Failure($"Ya existe una configuración activa para {provider}", "ProviderConfiguration.Provider");
        }

        _providerConfigurations.Remove(config);
        _providerConfigurations.Add(config with { IsActive = true });
        return Result.Success();
    }

    public PaymentProviderConfig? GetActiveConfigForProvider(string provider)
    {
        return _providerConfigurations.FirstOrDefault(p =>
            p.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase) && p.IsActive);
    }

    public bool HasActiveProviderConfiguration(string provider)
    {
        return _providerConfigurations.Any(p =>
            p.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase) && p.IsActive);
    }

    // Métodos de Actualización
    public Result Update(
        string name,
        string description,
        Money price,
        BillingPeriod billingPeriod)
    {
        Name = name;
        Description = description;
        Price = price;
        BillingPeriod = billingPeriod;

        var validation = ValidateEntity(this, new PlanValidator());
        return validation.IsFailure ? Result.Failure(validation.Errors) : Result.Success();
    }

    public Result Activate()
    {
        IsActive = true;
        return Result.Success();
    }

    public Result Deactivate()
    {
        IsActive = false;
        return Result.Success();
    }

    protected class PlanValidator : AbstractValidator<Plan>
    {
        public PlanValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La descripción es requerida")
                .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres");

            RuleFor(x => x.Price)
                .NotNull().WithMessage("El precio es requerido");

            RuleFor(x => x.Price.Amount)
                .GreaterThanOrEqualTo(0).WithMessage("El precio debe ser mayor o igual a 0");

            RuleFor(x => x.BillingPeriod)
                .IsInEnum().WithMessage("El período de facturación no es válido");

            RuleFor(x => x.Features)
                .NotEmpty().WithMessage("El plan debe tener al menos una característica");

            RuleFor(x => x.ProviderConfigurations)
                .NotEmpty().WithMessage("El plan debe tener al menos una configuración de proveedor");

            RuleFor(x => x)
                .Must(HaveAtLeastOneActiveProviderConfiguration)
                .WithMessage("El plan debe tener al menos una configuración de proveedor activa")
                .WithName("ProviderConfigurations");

            RuleFor(x => x)
                .Must(HaveUniqueFeatureCodes)
                .WithMessage("No puede haber Features con el mismo Code")
                .WithName("Features");

            RuleFor(x => x)
                .Must(HaveOnlyOneActiveConfigPerProvider)
                .WithMessage("No puede haber dos configuraciones activas para el mismo proveedor")
                .WithName("ProviderConfigurations");
        }

        private bool HaveAtLeastOneActiveProviderConfiguration(Plan plan)
        {
            return plan._providerConfigurations.Any(p => p.IsActive);
        }

        private bool HaveUniqueFeatureCodes(Plan plan)
        {
            return plan._features.Select(f => f.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                   == plan._features.Count;
        }

        private bool HaveOnlyOneActiveConfigPerProvider(Plan plan)
        {
            return plan._providerConfigurations
                .Where(p => p.IsActive)
                .GroupBy(p => p.Provider, StringComparer.OrdinalIgnoreCase)
                .All(g => g.Count() == 1);
        }
    }
}
