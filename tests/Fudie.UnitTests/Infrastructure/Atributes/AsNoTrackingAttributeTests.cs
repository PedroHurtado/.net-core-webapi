using FluentAssertions;
using Fudie.Infrastructure;

namespace Fudie.UnitTests.Infrastructure.Atributes;

public class AsNoTrackingAttributeTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldAlwaysDisableTracking()
    {
        // Arrange & Act
        var attribute = new AsNoTrackingAttribute();

        // Assert
        attribute.Enabled.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldCallBaseConstructorWithFalse()
    {
        // Arrange & Act
        var attribute = new AsNoTrackingAttribute();

        // Assert
        attribute.Enabled.Should().BeFalse("AsNoTrackingAttribute should always pass false to base constructor");
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void AsNoTrackingAttribute_ShouldInheritFromTrackingAttribute()
    {
        // Arrange
        var attributeType = typeof(AsNoTrackingAttribute);

        // Act & Assert
        attributeType.Should().BeAssignableTo<TrackingAttribute>();
    }

    [Fact]
    public void AsNoTrackingAttribute_ShouldInheritFromAttribute()
    {
        // Arrange
        var attributeType = typeof(AsNoTrackingAttribute);

        // Act & Assert
        attributeType.Should().BeAssignableTo<Attribute>();
    }

    [Fact]
    public void AsNoTrackingAttribute_ShouldBeSealed()
    {
        // Arrange
        var attributeType = typeof(AsNoTrackingAttribute);

        // Act & Assert
        attributeType.IsSealed.Should().BeTrue("AsNoTrackingAttribute should be sealed to prevent further inheritance");
    }

    #endregion

    #region Enabled Property Tests

    [Fact]
    public void Enabled_ShouldAlwaysReturnFalse()
    {
        // Arrange
        var attribute = new AsNoTrackingAttribute();

        // Act
        var result = attribute.Enabled;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Enabled_ShouldBeReadOnly()
    {
        // Arrange
        var attribute = new AsNoTrackingAttribute();

        // Act
        var enabled = attribute.Enabled;

        // Assert
        enabled.Should().BeFalse();
        // Verify property is read-only (inherited from base)
        var property = typeof(TrackingAttribute).GetProperty(nameof(TrackingAttribute.Enabled));
        property.Should().NotBeNull();
        property!.CanWrite.Should().BeFalse();
    }

    #endregion

    #region Attribute Usage Tests

    [Fact]
    public void AsNoTrackingAttribute_ShouldHaveCorrectAttributeUsage()
    {
        // Arrange
        var attributeType = typeof(AsNoTrackingAttribute);

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
    public void AsNoTrackingAttribute_ShouldBeApplicableToMethods()
    {
        // Arrange
        var attributeType = typeof(AsNoTrackingAttribute);

        // Act
        var attributeUsage = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        // Assert
        attributeUsage.Should().NotBeNull();
        attributeUsage!.ValidOn.HasFlag(AttributeTargets.Method).Should().BeTrue();
    }

    #endregion

    #region Semantic Equivalence Tests

    [Fact]
    public void AsNoTrackingAttribute_ShouldBeEquivalentToTrackingAttributeWithFalse()
    {
        // Arrange
        var asNoTracking = new AsNoTrackingAttribute();
        var trackingFalse = new TrackingAttribute(false);

        // Act & Assert
        asNoTracking.Enabled.Should().Be(trackingFalse.Enabled);
    }

    [Fact]
    public void AsNoTrackingAttribute_ShouldNotBeEquivalentToTrackingAttributeWithTrue()
    {
        // Arrange
        var asNoTracking = new AsNoTrackingAttribute();
        var trackingTrue = new TrackingAttribute(true);

        // Act & Assert
        asNoTracking.Enabled.Should().NotBe(trackingTrue.Enabled);
    }

    #endregion

    #region Multiple Instances Tests

    [Fact]
    public void MultipleInstances_ShouldAllHaveEnabledFalse()
    {
        // Arrange & Act
        var attribute1 = new AsNoTrackingAttribute();
        var attribute2 = new AsNoTrackingAttribute();
        var attribute3 = new AsNoTrackingAttribute();

        // Assert
        attribute1.Enabled.Should().BeFalse();
        attribute2.Enabled.Should().BeFalse();
        attribute3.Enabled.Should().BeFalse();
    }

    [Fact]
    public void MultipleInstances_ShouldBeIndependent()
    {
        // Arrange & Act
        var attributes = Enumerable.Range(0, 10)
            .Select(_ => new AsNoTrackingAttribute())
            .ToList();

        // Assert
        attributes.Should().AllSatisfy(a => a.Enabled.Should().BeFalse());
    }

    #endregion

    #region Type Compatibility Tests

    [Fact]
    public void AsNoTrackingAttribute_ShouldBeAssignableToTrackingAttribute()
    {
        // Arrange
        AsNoTrackingAttribute asNoTracking = new AsNoTrackingAttribute();

        // Act
        TrackingAttribute tracking = asNoTracking;

        // Assert
        tracking.Should().NotBeNull();
        tracking.Enabled.Should().BeFalse();
    }

    [Fact]
    public void AsNoTrackingAttribute_CanBeUsedWhereTrackingAttributeIsExpected()
    {
        // Arrange
        var attribute = new AsNoTrackingAttribute();

        // Act
        var isTrackingAttribute = attribute is TrackingAttribute;

        // Assert
        isTrackingAttribute.Should().BeTrue();
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void Enabled_ValueShouldNeverChange()
    {
        // Arrange
        var attribute = new AsNoTrackingAttribute();
        var initialValue = attribute.Enabled;

        // Act
        var currentValue = attribute.Enabled;

        // Assert
        currentValue.Should().Be(initialValue);
        currentValue.Should().BeFalse();
    }

    #endregion

    #region Constructor Behavior Tests

    [Fact]
    public void Constructor_ShouldNotAcceptParameters()
    {
        // Arrange
        var constructors = typeof(AsNoTrackingAttribute).GetConstructors();

        // Act
        var parameterlessConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);

        // Assert
        parameterlessConstructor.Should().NotBeNull();
        constructors.Should().HaveCount(1, "AsNoTrackingAttribute should only have a parameterless constructor");
    }

    #endregion

    #region Semantic Clarity Tests

    [Fact]
    public void AsNoTrackingAttribute_NameShouldReflectPurpose()
    {
        // Arrange
        var attribute = new AsNoTrackingAttribute();

        // Act & Assert
        attribute.Enabled.Should().BeFalse("'AsNoTracking' semantically means tracking is disabled");
    }

    [Fact]
    public void AsNoTrackingAttribute_ShouldProvideReadableAlternativeToTrackingFalse()
    {
        // This test documents the design intent: AsNoTracking is more readable than Tracking(false)

        // Arrange
        var asNoTracking = new AsNoTrackingAttribute();
        var trackingFalse = new TrackingAttribute(false);

        // Act & Assert
        asNoTracking.Enabled.Should().Be(trackingFalse.Enabled);
        // Both achieve the same result, but AsNoTracking is more expressive
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void AsNoTrackingAttribute_ShouldWorkInCollections()
    {
        // Arrange & Act
        var attributes = new List<AsNoTrackingAttribute>
        {
            new AsNoTrackingAttribute(),
            new AsNoTrackingAttribute(),
            new AsNoTrackingAttribute()
        };

        // Assert
        attributes.Should().HaveCount(3);
        attributes.Should().AllSatisfy(a => a.Enabled.Should().BeFalse());
    }

    [Fact]
    public void AsNoTrackingAttribute_ShouldWorkInArrays()
    {
        // Arrange & Act
        var attributes = new AsNoTrackingAttribute[]
        {
            new AsNoTrackingAttribute(),
            new AsNoTrackingAttribute()
        };

        // Assert
        attributes.Should().HaveCount(2);
        attributes.Should().AllSatisfy(a => a.Enabled.Should().BeFalse());
    }

    #endregion

    #region Polymorphism Tests

    [Fact]
    public void AsNoTrackingAttribute_WhenCastToBase_ShouldMaintainBehavior()
    {
        // Arrange
        AsNoTrackingAttribute derived = new AsNoTrackingAttribute();
        TrackingAttribute baseAttribute = derived;

        // Act & Assert
        baseAttribute.Enabled.Should().BeFalse();
        derived.Enabled.Should().BeFalse();
        baseAttribute.Enabled.Should().Be(derived.Enabled);
    }

    #endregion
}
