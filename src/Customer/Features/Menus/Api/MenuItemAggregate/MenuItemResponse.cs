namespace Customer.Features.Menus.Api.MenuItemAggregate;

public record MenuItemResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    bool IsAvailable,
    bool IsHighRiskItem,
    bool RequiresAdvanceOrder,
    int? MinimumAdvanceOrderQuantity,
    bool IsAlwaysAvailable,
    string? AllergenNotes,
    bool IsAvailableToday,
    bool CanBeOrdered,
    bool HasDepositOverride,
    ItemDepositOverrideResponse? DepositOverride,
    NutritionalInfoResponse? NutritionalInfo,
    IReadOnlyCollection<PriceOptionResponse> PriceOptions,
    IReadOnlyCollection<DayOfWeek> AvailableDays,
    IReadOnlyCollection<AllergenResponse> Allergens)
{
    public static MenuItemResponse Map(MenuItem menuItem) => new(
        Id: menuItem.Id,
        TenantId: menuItem.TenantId,
        Name: menuItem.Name,
        Description: menuItem.Description,
        ImageUrl: menuItem.ImageUrl,
        DisplayOrder: menuItem.DisplayOrder,
        IsActive: menuItem.IsActive,
        IsAvailable: menuItem.IsAvailable,
        IsHighRiskItem: menuItem.IsHighRiskItem,
        RequiresAdvanceOrder: menuItem.RequiresAdvanceOrder,
        MinimumAdvanceOrderQuantity: menuItem.MinimumAdvanceOrderQuantity,
        IsAlwaysAvailable: menuItem.IsAlwaysAvailable,
        AllergenNotes: menuItem.AllergenNotes,
        IsAvailableToday: menuItem.IsAvailableToday,
        CanBeOrdered: menuItem.CanBeOrdered,
        HasDepositOverride: menuItem.HasDepositOverride,
        DepositOverride: menuItem.DepositOverride is not null
            ? ItemDepositOverrideResponse.Map(menuItem.DepositOverride)
            : null,
        NutritionalInfo: menuItem.NutritionalInfo is not null
            ? NutritionalInfoResponse.Map(menuItem.NutritionalInfo)
            : null,
        PriceOptions: menuItem.PriceOptions
            .Select(PriceOptionResponse.Map)
            .ToList()
            .AsReadOnly(),
        AvailableDays: menuItem.AvailableDays,
        Allergens: menuItem.Allergens
            .Select(AllergenResponse.Map)
            .ToList()
            .AsReadOnly());
}

public record PriceOptionResponse(
    PortionType PortionType,
    decimal? Price,
    bool IsActive,
    bool RequiresMarketPrice,
    string DisplayPrice)
{
    public static PriceOptionResponse Map(PriceOption priceOption) => new(
        PortionType: priceOption.PortionType,
        Price: priceOption.Price,
        IsActive: priceOption.IsActive,
        RequiresMarketPrice: priceOption.RequiresMarketPrice,
        DisplayPrice: priceOption.DisplayPrice);
}

public record ItemDepositOverrideResponse(
    decimal DepositAmount,
    int? MinimumQuantityForDeposit,
    bool AppliesToAllQuantities)
{
    public static ItemDepositOverrideResponse Map(ItemDepositOverride depositOverride) => new(
        DepositAmount: depositOverride.DepositAmount,
        MinimumQuantityForDeposit: depositOverride.MinimumQuantityForDeposit,
        AppliesToAllQuantities: depositOverride.AppliesToAllQuantities);
}

public record NutritionalInfoResponse(
    int Calories,
    decimal Protein,
    decimal Carbohydrates,
    decimal Fat,
    decimal? Fiber,
    decimal? Sugar,
    decimal? Salt,
    int ServingSize)
{
    public static NutritionalInfoResponse Map(NutritionalInfo nutritionalInfo) => new(
        Calories: nutritionalInfo.Calories,
        Protein: nutritionalInfo.Protein,
        Carbohydrates: nutritionalInfo.Carbohydrates,
        Fat: nutritionalInfo.Fat,
        Fiber: nutritionalInfo.Fiber,
        Sugar: nutritionalInfo.Sugar,
        Salt: nutritionalInfo.Salt,
        ServingSize: nutritionalInfo.ServingSize);
}

public record AllergenResponse(
    string Id,
    string Name,
    string? IconUrl)
{
    public static AllergenResponse Map(Allergen allergen) => new(
        Id: allergen.Id,
        Name: allergen.Name,
        IconUrl: allergen.IconUrl);
}
