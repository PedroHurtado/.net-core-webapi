using FluentAssertions;
using Fudie.Infrastructure;

namespace Fudie.UnitTests.Infrastructure.Atributes;

public class TrackingAttributeTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithDefaultParameter_ShouldEnableTracking()
    {
        // Arrange & Act
        var attribute = new TrackingAttribute();

        // Assert
        attribute.Enabled.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithTrueParameter_ShouldEnableTracking()
    {
        // Arrange & Act
        var attribute = new TrackingAttribute(true);

        // Assert
        attribute.Enabled.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithFalseParameter_ShouldDisableTracking()
    {
        // Arrange & Act
        var attribute = new TrackingAttribute(false);

        // Assert
        attribute.Enabled.Should().BeFalse();
    }

    #endregion

    #region Enabled Property Tests

    [Fact]
    public void Enabled_WhenConstructedWithTrue_ShouldReturnTrue()
    {
        // Arrange
        var attribute = new TrackingAttribute(enabled: true);

        // Act
        var result = attribute.Enabled;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Enabled_WhenConstructedWithFalse_ShouldReturnFalse()
    {
        // Arrange
        var attribute = new TrackingAttribute(enabled: false);

        // Act
        var result = attribute.Enabled;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Enabled_ShouldBeReadOnly()
    {
        // Arrange
        var attribute = new TrackingAttribute(true);

        // Act
        var enabled = attribute.Enabled;

        // Assert
        enabled.Should().BeTrue();
        // Verify property is read-only by checking it doesn't have a setter
        var property = typeof(TrackingAttribute).GetProperty(nameof(TrackingAttribute.Enabled));
        property.Should().NotBeNull();
        property!.CanWrite.Should().BeFalse();
    }

    #endregion

    #region Attribute Usage Tests

    [Fact]
    public void TrackingAttribute_ShouldHaveCorrectAttributeUsage()
    {
        // Arrange
        var attributeType = typeof(TrackingAttribute);

        // Act
        var attributeUsage = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        // Assert
        attributeUsage.Should().NotBeNull();
        attributeUsage!.ValidOn.Should().Be(AttributeTargets.Interface | AttributeTargets.Method);
        attributeUsage.AllowMultiple.Should().BeFalse();
        attributeUsage.Inherited.Should().BeFalse();
    }

    [Fact]
    public void TrackingAttribute_ShouldBeApplicableToMethods()
    {
        // Arrange
        var attributeType = typeof(TrackingAttribute);

        // Act
        var attributeUsage = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        // Assert
        attributeUsage.Should().NotBeNull();
        attributeUsage!.ValidOn.HasFlag(AttributeTargets.Method).Should().BeTrue();
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void TrackingAttribute_ShouldInheritFromAttribute()
    {
        // Arrange
        var attributeType = typeof(TrackingAttribute);

        // Act & Assert
        attributeType.Should().BeAssignableTo<Attribute>();
    }

    #endregion

    #region Multiple Instances Tests

    [Fact]
    public void MultipleInstances_WithDifferentValues_ShouldBeIndependent()
    {
        // Arrange & Act
        var attribute1 = new TrackingAttribute(true);
        var attribute2 = new TrackingAttribute(false);

        // Assert
        attribute1.Enabled.Should().BeTrue();
        attribute2.Enabled.Should().BeFalse();
    }

    [Fact]
    public void MultipleInstances_WithSameValue_ShouldHaveSameEnabled()
    {
        // Arrange & Act
        var attribute1 = new TrackingAttribute(true);
        var attribute2 = new TrackingAttribute(true);

        // Assert
        attribute1.Enabled.Should().Be(attribute2.Enabled);
    }

    #endregion

    #region Default Behavior Tests

    [Fact]
    public void Constructor_WithoutParameters_ShouldDefaultToEnabled()
    {
        // Arrange & Act
        var attribute = new TrackingAttribute();

        // Assert
        attribute.Enabled.Should().BeTrue("default parameter value should be true");
    }

    #endregion

    #region Type Safety Tests

    [Fact]
    public void Enabled_ShouldBeBooleanType()
    {
        // Arrange
        var attribute = new TrackingAttribute();

        // Act
        var enabledProperty = typeof(TrackingAttribute).GetProperty(nameof(TrackingAttribute.Enabled));

        // Assert
        enabledProperty.Should().NotBeNull();
        enabledProperty!.PropertyType.Should().Be(typeof(bool));
    }

    #endregion

    #region Semantic Tests

    [Theory]
    [InlineData(true, "tracking enabled")]
    [InlineData(false, "tracking disabled")]
    public void TrackingAttribute_ShouldRepresentCorrectSemantics(bool enabled, string expectedSemantics)
    {
        // Arrange & Act
        var attribute = new TrackingAttribute(enabled);

        // Assert
        if (expectedSemantics.Contains("enabled"))
        {
            attribute.Enabled.Should().BeTrue();
        }
        else
        {
            attribute.Enabled.Should().BeFalse();
        }
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void Enabled_ValueShouldNotChangeAfterConstruction()
    {
        // Arrange
        var attribute = new TrackingAttribute(true);
        var initialValue = attribute.Enabled;

        // Act
        var currentValue = attribute.Enabled;

        // Assert
        currentValue.Should().Be(initialValue);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_CalledMultipleTimes_ShouldCreateIndependentInstances()
    {
        // Arrange & Act
        var attributes = Enumerable.Range(0, 100)
            .Select(i => new TrackingAttribute(i % 2 == 0))
            .ToList();

        // Assert
        attributes.Count(a => a.Enabled).Should().Be(50);
        attributes.Count(a => !a.Enabled).Should().Be(50);
    }

    #endregion
}
