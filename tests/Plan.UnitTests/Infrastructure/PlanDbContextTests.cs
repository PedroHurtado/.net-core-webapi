using Microsoft.EntityFrameworkCore;
using Plan.Infrastructure;

namespace Plan.UnitTests.Infrastructure;

public class PlanDbContextTests
{
    [Fact]
    public void PlanDbContext_ShouldBeCreated_Successfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PlanDbContext>()
            .UseInMemoryDatabase(databaseName: "TestPlanDb")
            .Options;

        // Act
        using var context = new PlanDbContext(options);

        // Assert
        context.Should().NotBeNull();
    }
}
