using Fudie.Firestore.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Plan.Infrastructure;

namespace Plan.UnitTests.Infrastructure;

public class PlanDbContextTests
{
    private const string EmulatorHost = "127.0.0.1:8080";

    private readonly PlanDbContext _context;
    private readonly IModel _model;

    public PlanDbContextTests()
    {
        Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", EmulatorHost);
        var options = new DbContextOptionsBuilder<PlanDbContext>()
            .UseFirestore("test-project")
            .Options;

        _context = new PlanDbContext(options);
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
    public void Model_ShouldHavePlanEntityType()
    {
        var entityType = _model.FindEntityType(typeof(global::Plan.Features.Plans.Domain.PlanAggregate.Plan));

        entityType.Should().NotBeNull();
    }

    [Fact]
    public void Plan_ShouldNotHaveQueryFilter()
    {
        var entityType = _model.FindEntityType(typeof(global::Plan.Features.Plans.Domain.PlanAggregate.Plan))!;

        entityType.GetQueryFilter().Should().BeNull();
    }

    [Fact]
    public void DbContext_ShouldExposePlansDbSet()
    {
        _context.Plans.Should().NotBeNull();
    }
}
