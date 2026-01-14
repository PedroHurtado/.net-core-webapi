using Fudie.Domain;
using Fudie;
using FluentValidation;

namespace Schedule.Features.Menu.Models;

public class Menu : Entity<Guid>
{
    public Guid RestaurantId { get; protected set; }
    public string Name { get; protected set; }
    public string? Description { get; protected set; }
    public bool IsActive { get; protected set; }
    public DateTime? EffectiveFrom { get; protected set; }
    public DateTime? EffectiveUntil { get; protected set; }
    public int DisplayOrder { get; protected set; }

    public IReadOnlyCollection<MenuCategory> Categories => _categories.ToList().AsReadOnly();
    protected HashSet<MenuCategory> _categories = [];

    public DepositPolicy? DepositPolicy { get; protected set; }

    protected Menu(Guid id, Guid restaurantId, string name, string? description, DateTime? effectiveFrom, DateTime? effectiveUntil) : base(id)
    {
        RestaurantId = restaurantId;
        Name = name;
        Description = description;
        EffectiveFrom = effectiveFrom;
        EffectiveUntil = effectiveUntil;
        IsActive = true;
    }

    public static Result<Menu> Create(Guid id, Guid restaurantId, string name, string? description = null, DateTime? effectiveFrom = null, DateTime? effectiveUntil = null)
    {
        var menu = new Menu(id, restaurantId, name, description, effectiveFrom, effectiveUntil);
        var validation = menu.ValidateEntity(menu, new MenuValidator());

        if (validation.IsFailure)
            return Result<Menu>.Failure(validation.Errors);

        return Result<Menu>.Success(menu);
    }

    public Result SetDepositPolicy(DepositType depositType, decimal amount, decimal? percentage = null, decimal? minimumBill = null, int? minimumGuests = null)
    {
        var policy = new DepositPolicy(depositType, amount, percentage, minimumBill, minimumGuests);
        
        if (depositType == DepositType.PercentageOfBill && (!percentage.HasValue || percentage < 1 || percentage > 100))
            return Result.Failure("Percentage must be between 1 and 100 for PercentageOfBill type", nameof(percentage));
            
        if (depositType != DepositType.PercentageOfBill && percentage.HasValue)
             return Result.Failure("Percentage should be null for this deposit type", nameof(percentage));

        if (amount <= 0 && depositType != DepositType.PercentageOfBill)
             return Result.Failure("Amount must be greater than zero", nameof(amount));

        DepositPolicy = policy;
        return Result.Success();
    }

    public Result RemoveDepositPolicy()
    {
        DepositPolicy = null;
        return Result.Success();
    }

    public Result AddCategory(string name, string? description = null)
    {
        if (_categories.Any(c => c.Name == name))
            return Result.Failure("Category with this name already exists", nameof(name));

        var category = new MenuCategory(Guid.NewGuid(), name, description, _categories.Count + 1);
        
        if (string.IsNullOrWhiteSpace(name))
             return Result.Failure("Name is required", nameof(name));
             
        if (name.Length > 100)
             return Result.Failure("Name exceeds maximum length", nameof(name));

        _categories.Add(category);
        return Result.Success();
    }

    protected class MenuValidator : AbstractValidator<Menu>
    {
        public MenuValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty().WithMessage("RestaurantId is required");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required").MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(500);
            RuleFor(x => x.EffectiveFrom)
                .LessThan(x => x.EffectiveUntil)
                .When(x => x.EffectiveFrom.HasValue && x.EffectiveUntil.HasValue)
                .WithMessage("EffectiveFrom must be before EffectiveUntil");
        }
    }
}

public class MenuCategory(Guid id, string name, string? description, int displayOrder) : Entity<Guid>(id)
{
    public string Name { get; protected set; } = name;
    public string? Description { get; protected set; } = description;
    public int DisplayOrder { get; protected set; } = displayOrder;
    public bool IsActive { get; protected set; } = true;

    public IReadOnlyCollection<MenuItem> Items => _items.ToList().AsReadOnly();
    protected HashSet<MenuItem> _items = [];

    public Result AddItem(string name, string? description, List<PriceOption> priceOptions)
    {
        if (_items.Any(i => i.Name == name))
            return Result.Failure("Item with this name already exists in this category", nameof(name));
            
        if (priceOptions == null || priceOptions.Count == 0)
            return Result.Failure("Item must have at least one price option", nameof(priceOptions));

        var item = new MenuItem(Guid.NewGuid(), name, description, priceOptions);
        _items.Add(item);
        return Result.Success();
    }
}

public class MenuItem(Guid id, string name, string? description, List<PriceOption> priceOptions) : Entity<Guid>(id)
{
    public string Name { get; protected set; } = name;
    public string? Description { get; protected set; } = description;
    public bool IsActive { get; protected set; } = true;
    public bool IsAvailable { get; protected set; } = true;

    public IReadOnlyCollection<PriceOption> PriceOptions => _priceOptions.ToList().AsReadOnly();
    protected HashSet<PriceOption> _priceOptions = new HashSet<PriceOption>(priceOptions);
}

public record DepositPolicy(DepositType DepositType, decimal Amount, decimal? Percentage, decimal? MinimumBillForDeposit, int? MinimumGuestsForDeposit);

public enum DepositType
{
    PerPerson = 1,
    PercentageOfBill = 2,
    FixedAmount = 3
}

public record PriceOption(Guid Id, PortionType PortionType, decimal? Price);

public enum PortionType
{
    Tapa = 1,
    MediaRacion = 2,
    Racion = 3,
    SegunMercado = 4
}
