using FluentAssertions;
using Fudie.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fudie.UnitTests.DependencyInjection;

public class InjectableAttributeTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithDefaultParameters_ShouldSetScopedLifetime()
    {
        // Act
        var attribute = new InjectableAttribute();

        // Assert
        attribute.Lifetime.Should().Be(ServiceLifetime.Scoped);
        attribute.ServiceType.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithTransientLifetime_ShouldSetTransientLifetime()
    {
        // Act
        var attribute = new InjectableAttribute(ServiceLifetime.Transient);

        // Assert
        attribute.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void Constructor_WithScopedLifetime_ShouldSetScopedLifetime()
    {
        // Act
        var attribute = new InjectableAttribute(ServiceLifetime.Scoped);

        // Assert
        attribute.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void Constructor_WithSingletonLifetime_ShouldSetSingletonLifetime()
    {
        // Act
        var attribute = new InjectableAttribute(ServiceLifetime.Singleton);

        // Assert
        attribute.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    #endregion

    #region ServiceType Property Tests

    [Fact]
    public void ServiceType_WithInitializer_ShouldSetServiceType()
    {
        // Arrange
        var expectedType = typeof(ITestService);

        // Act
        var attribute = new InjectableAttribute { ServiceType = expectedType };

        // Assert
        attribute.ServiceType.Should().Be(expectedType);
    }

    [Fact]
    public void ServiceType_WithoutInitializer_ShouldBeNull()
    {
        // Act
        var attribute = new InjectableAttribute();

        // Assert
        attribute.ServiceType.Should().BeNull();
    }

    [Fact]
    public void ServiceType_WithNullValue_ShouldAcceptNull()
    {
        // Act
        var attribute = new InjectableAttribute { ServiceType = null };

        // Assert
        attribute.ServiceType.Should().BeNull();
    }

    #endregion

    #region Lifetime Property Tests

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Singleton)]
    public void Lifetime_WithInitializer_ShouldSetLifetime(ServiceLifetime lifetime)
    {
        // Act
        var attribute = new InjectableAttribute { Lifetime = lifetime };

        // Assert
        attribute.Lifetime.Should().Be(lifetime);
    }

    [Fact]
    public void Lifetime_CanBeOverriddenWithInitializer()
    {
        // Act
        var attribute = new InjectableAttribute(ServiceLifetime.Transient)
        {
            Lifetime = ServiceLifetime.Singleton
        };

        // Assert
        attribute.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    #endregion

    #region Attribute Usage Tests

    [Fact]
    public void InjectableAttribute_ShouldHaveCorrectAttributeUsage()
    {
        // Arrange
        var attributeType = typeof(InjectableAttribute);

        // Act
        var attributeUsage = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        // Assert
        attributeUsage.Should().NotBeNull();
        attributeUsage!.ValidOn.Should().Be(AttributeTargets.Class);
        attributeUsage.AllowMultiple.Should().BeTrue();
        attributeUsage.Inherited.Should().BeFalse();
    }

    [Fact]
    public void InjectableAttribute_ShouldBeSealed()
    {
        // Arrange
        var attributeType = typeof(InjectableAttribute);

        // Act & Assert
        attributeType.IsSealed.Should().BeTrue("InjectableAttribute should be sealed to prevent inheritance");
    }

    [Fact]
    public void InjectableAttribute_ShouldInheritFromAttribute()
    {
        // Arrange
        var attributeType = typeof(InjectableAttribute);

        // Act & Assert
        attributeType.Should().BeAssignableTo<Attribute>();
    }

    #endregion

    #region Combined Property Tests

    [Fact]
    public void InjectableAttribute_WithAllProperties_ShouldSetAllCorrectly()
    {
        // Arrange
        var expectedType = typeof(ITestService);
        var expectedLifetime = ServiceLifetime.Singleton;

        // Act
        var attribute = new InjectableAttribute(ServiceLifetime.Transient)
        {
            Lifetime = expectedLifetime,
            ServiceType = expectedType
        };

        // Assert
        attribute.Lifetime.Should().Be(expectedLifetime);
        attribute.ServiceType.Should().Be(expectedType);
    }

    [Fact]
    public void InjectableAttribute_MultipleInstances_ShouldBeIndependent()
    {
        // Act
        var attribute1 = new InjectableAttribute(ServiceLifetime.Transient)
        {
            ServiceType = typeof(ITestService)
        };
        var attribute2 = new InjectableAttribute(ServiceLifetime.Singleton)
        {
            ServiceType = typeof(IAnotherService)
        };

        // Assert
        attribute1.Lifetime.Should().Be(ServiceLifetime.Transient);
        attribute1.ServiceType.Should().Be(typeof(ITestService));
        attribute2.Lifetime.Should().Be(ServiceLifetime.Singleton);
        attribute2.ServiceType.Should().Be(typeof(IAnotherService));
    }

    #endregion

    #region Practical Usage Tests

    [Fact]
    public void InjectableAttribute_CanBeAppliedToClass()
    {
        // Arrange
        var testServiceType = typeof(TestService);

        // Act
        var attribute = testServiceType.GetCustomAttributes(typeof(InjectableAttribute), false)
            .Cast<InjectableAttribute>()
            .FirstOrDefault();

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void InjectableAttribute_WithCustomLifetime_CanBeAppliedToClass()
    {
        // Arrange
        var singletonServiceType = typeof(SingletonService);

        // Act
        var attribute = singletonServiceType.GetCustomAttributes(typeof(InjectableAttribute), false)
            .Cast<InjectableAttribute>()
            .FirstOrDefault();

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    #endregion

    #region Test Helper Classes and Interfaces

    private interface ITestService { }
    private interface IAnotherService { }

    [Injectable]
    private class TestService : ITestService { }

    [Injectable(ServiceLifetime.Singleton)]
    private class SingletonService : ITestService { }

    #endregion
}
