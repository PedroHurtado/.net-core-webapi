using FluentAssertions;
using Fudie.Features;

namespace Fudie.UnitTests.Features;

public class IAggregateDescriptionTests
{
    #region Interface Tests

    [Fact]
    public void IAggregateDescription_ShouldBeAnInterface()
    {
        // Arrange
        var type = typeof(IAggregateDescription);

        // Act & Assert
        type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IAggregateDescription_ShouldBePublic()
    {
        // Arrange
        var type = typeof(IAggregateDescription);

        // Act & Assert
        type.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void IAggregateDescription_ShouldHaveIdProperty()
    {
        // Arrange
        var type = typeof(IAggregateDescription);

        // Act
        var property = type.GetProperty(nameof(IAggregateDescription.Id));

        // Assert
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(string));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IAggregateDescription_ShouldHaveDisplayNameProperty()
    {
        // Arrange
        var type = typeof(IAggregateDescription);

        // Act
        var property = type.GetProperty(nameof(IAggregateDescription.DisplayName));

        // Assert
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(string));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IAggregateDescription_ShouldHaveIconProperty()
    {
        // Arrange
        var type = typeof(IAggregateDescription);

        // Act
        var property = type.GetProperty(nameof(IAggregateDescription.Icon));

        // Assert
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(string));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IAggregateDescription_ShouldHaveExactlyThreeProperties()
    {
        // Arrange
        var type = typeof(IAggregateDescription);

        // Act
        var properties = type.GetProperties();

        // Assert
        properties.Should().HaveCount(3);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void IAggregateDescription_CanBeImplemented()
    {
        // Arrange & Act
        var implementation = new TestAggregateDescription();

        // Assert
        implementation.Should().BeAssignableTo<IAggregateDescription>();
    }

    [Fact]
    public void IAggregateDescription_Implementation_ReturnsCorrectValues()
    {
        // Arrange
        var implementation = new TestAggregateDescription();

        // Act & Assert
        implementation.Id.Should().Be("menu");
        implementation.DisplayName.Should().Be("Menús");
        implementation.Icon.Should().Be("book-open");
    }

    [Fact]
    public void IAggregateDescription_Implementation_IconCanBeNull()
    {
        // Arrange
        var implementation = new TestAggregateDescriptionWithoutIcon();

        // Act & Assert
        implementation.Icon.Should().BeNull();
    }

    #endregion

    #region Helper Classes

    private class TestAggregateDescription : IAggregateDescription
    {
        public string Id => "menu";
        public string DisplayName => "Menús";
        public string? Icon => "book-open";
    }

    private class TestAggregateDescriptionWithoutIcon : IAggregateDescription
    {
        public string Id => "session";
        public string DisplayName => "Sesiones";
        public string? Icon => null;
    }

    #endregion
}
