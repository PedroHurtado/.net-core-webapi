using Customer.Infrastructure;
using Fudie.Firestore.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Customer.UnitTests.Infrastructure;

public class CustomerDbContextTests
{
    private const string EmulatorHost = "127.0.0.1:8080";

    private readonly CustomerDbContext _context;
    private readonly IModel _model;

    public CustomerDbContextTests()
    {
        Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", EmulatorHost);
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseFirestore("test-project")
            .Options;

        _context = new CustomerDbContext(options, Guid.NewGuid());
        _model = _context.Model;        
    }

    [Fact]
    public void Model_ShouldUsePropertyAccessModeField()
    {
        var designTimeModel = _context.GetService<IDesignTimeModel>().Model;
        var accessMode = designTimeModel.GetPropertyAccessMode();

        accessMode.Should().Be(PropertyAccessMode.Field);
    }

    [Fact]
    public void Model_ShouldHaveMenuEntityType()
    {
        var entityType = _model.FindEntityType(typeof(MenuAgg));

        entityType.Should().NotBeNull();
    }

    [Fact]
    public void Model_ShouldHaveMenuItemEntityType()
    {
        var entityType = _model.FindEntityType(typeof(MenuItemAgg));

        entityType.Should().NotBeNull();
    }

    [Fact]
    public void Model_ShouldHaveAllergenEntityType()
    {
        var entityType = _model.FindEntityType(typeof(Allergen));

        entityType.Should().NotBeNull();
    }

    [Fact]
    public void Menu_ShouldHaveQueryFilter()
    {
        var entityType = _model.FindEntityType(typeof(MenuAgg))!;

        entityType.GetQueryFilter().Should().NotBeNull();
    }

    [Fact]
    public void Menu_ShouldHaveDepositPolicyAsComplexProperty()
    {
        var entityType = _model.FindEntityType(typeof(MenuAgg))!;
        var complexProperty = entityType.FindComplexProperty(nameof(MenuAgg.DepositPolicy));

        complexProperty.Should().NotBeNull();
    }

    [Fact]
    public void Menu_ShouldHaveCategoriesNavigation()
    {
        var entityType = _model.FindEntityType(typeof(MenuAgg))!;
        var navigation = entityType.FindNavigation(nameof(MenuAgg.Categories));

        navigation.Should().NotBeNull();
    }

    [Fact]
    public void MenuItem_ShouldHaveQueryFilter()
    {
        var entityType = _model.FindEntityType(typeof(MenuItemAgg))!;

        entityType.GetQueryFilter().Should().NotBeNull();
    }

    [Fact]
    public void MenuItem_ShouldHaveDepositOverrideAsComplexProperty()
    {
        var entityType = _model.FindEntityType(typeof(MenuItemAgg))!;
        var complexProperty = entityType.FindComplexProperty(nameof(MenuItemAgg.DepositOverride));

        complexProperty.Should().NotBeNull();
    }

    [Fact]
    public void MenuItem_ShouldHaveNutritionalInfoAsComplexProperty()
    {
        var entityType = _model.FindEntityType(typeof(MenuItemAgg))!;
        var complexProperty = entityType.FindComplexProperty(nameof(MenuItemAgg.NutritionalInfo));

        complexProperty.Should().NotBeNull();
    }

    [Fact]
    public void DbContext_ShouldExposeMenusDbSet()
    {
        _context.Menus.Should().NotBeNull();
    }

    [Fact]
    public void DbContext_ShouldExposeMenuItemsDbSet()
    {
        _context.MenuItems.Should().NotBeNull();
    }

    [Fact]
    public void DbContext_ShouldExposeAllergensDbSet()
    {
        _context.Allergens.Should().NotBeNull();
    }
}