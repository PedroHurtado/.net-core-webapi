namespace Menus.Features.Menus.Api.MenuAggregate;

public record MenuResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder,
    DateTime? EffectiveFrom,
    DateTime? EffectiveUntil,
    DepositPolicyResponse? DepositPolicy,
    IReadOnlyCollection<MenuCategoryResponse> Categories)
{
    public static MenuResponse Map(Menu menu) => new(
        Id: menu.Id,
        Name: menu.Name,
        Description: menu.Description,
        IsActive: menu.IsActive,
        DisplayOrder: menu.DisplayOrder,
        EffectiveFrom: menu.EffectiveFrom,
        EffectiveUntil: menu.EffectiveUntil,
        DepositPolicy: menu.DepositPolicy is not null
            ? DepositPolicyResponse.Map(menu.DepositPolicy)
            : null,
        Categories: menu.Categories
            .Select(MenuCategoryResponse.Map)
            .ToList()
            .AsReadOnly());
}

public record DepositPolicyResponse(
    DepositType DepositType,
    decimal Amount,
    decimal? Percentage,
    decimal? MinimumBillForDeposit,
    int? MinimumGuestsForDeposit)
{
    public static DepositPolicyResponse Map(DepositPolicy depositPolicy) => new(
        DepositType: depositPolicy.DepositType,
        Amount: depositPolicy.Amount,
        Percentage: depositPolicy.Percentage,
        MinimumBillForDeposit: depositPolicy.MinimumBillForDeposit,
        MinimumGuestsForDeposit: depositPolicy.MinimumGuestsForDeposit);
}

public record MenuCategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyCollection<CategoryItemResponse> Items)
{
    public static MenuCategoryResponse Map(MenuCategory category) => new(
        Id: category.Id,
        Name: category.Name,
        Description: category.Description,
        DisplayOrder: category.DisplayOrder,
        IsActive: category.IsActive,
        Items: category.Items
            .Select(CategoryItemResponse.Map)
            .ToList()
            .AsReadOnly());
}

public record CategoryItemResponse(
    MenuItemSummaryResponse MenuItem,
    int DisplayOrder,
    IReadOnlyCollection<PriceOptionResponse> PriceOverrides)
{
    public static CategoryItemResponse Map(CategoryItem categoryItem) => new(
        MenuItem: MenuItemSummaryResponse.Map(categoryItem.MenuItem),
        DisplayOrder: categoryItem.DisplayOrder,
        PriceOverrides: categoryItem.PriceOverrides
            .Select(PriceOptionResponse.Map)
            .ToList()
            .AsReadOnly());
}

public record MenuItemSummaryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    bool IsActive,
    bool IsAvailable)
{
    public static MenuItemSummaryResponse Map(MenuItem menuItem) => new(
        Id: menuItem.Id,
        Name: menuItem.Name,
        Description: menuItem.Description,
        ImageUrl: menuItem.ImageUrl,
        IsActive: menuItem.IsActive,
        IsAvailable: menuItem.IsAvailable);
}