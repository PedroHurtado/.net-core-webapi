using FluentAssertions;
using Fudie.Infrastructure;

namespace Fudie.UnitTests.Infrastructure.Atributes;

// Clases de ejemplo para los tests
public class TestEntity { }
public class AnotherEntity { }

public class GenerateRepositoryAttributeTests
{
    #region Constructor Tests - Single Generic Parameter

    [Fact]
    public void Constructor_WithSingleGenericParameter_ShouldCreateInstance()
    {
        // Arrange & Act
        var attribute = new GenerateRepositoryAttribute<TestEntity>();

        // Assert
        attribute.Should().NotBeNull();
    }

    #endregion

    #region Constructor Tests - Two Generic Parameters

    [Fact]
    public void Constructor_WithTwoGenericParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var attribute = new GenerateRepositoryAttribute<TestEntity, Guid>();

        // Assert
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithIntId_ShouldCreateInstance()
    {
        // Arrange & Act
        var attribute = new GenerateRepositoryAttribute<TestEntity, int>();

        // Assert
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithStringId_ShouldCreateInstance()
    {
        // Arrange & Act
        var attribute = new GenerateRepositoryAttribute<TestEntity, string>();

        // Assert
        attribute.Should().NotBeNull();
    }

    #endregion

    #region Attribute Usage Tests - Single Generic

    [Fact]
    public void GenerateRepositoryAttribute_SingleGeneric_ShouldHaveCorrectAttributeUsage()
    {
        // Arrange
        var attributeType = typeof(GenerateRepositoryAttribute<>);

        // Act
        var attributeUsage = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        // Assert
        attributeUsage.Should().NotBeNull();
        attributeUsage!.ValidOn.Should().Be(AttributeTargets.Interface);
        attributeUsage.AllowMultiple.Should().BeFalse();
        attributeUsage.Inherited.Should().BeFalse();
    }

    #endregion

    #region Attribute Usage Tests - Two Generics

    [Fact]
    public void GenerateRepositoryAttribute_TwoGenerics_ShouldHaveCorrectAttributeUsage()
    {
        // Arrange
        var attributeType = typeof(GenerateRepositoryAttribute<,>);

        // Act
        var attributeUsage = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        // Assert
        attributeUsage.Should().NotBeNull();
        attributeUsage!.ValidOn.Should().Be(AttributeTargets.Interface);
        attributeUsage.AllowMultiple.Should().BeFalse();
        attributeUsage.Inherited.Should().BeFalse();
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void GenerateRepositoryAttribute_SingleGeneric_ShouldInheritFromAttribute()
    {
        // Arrange
        var attributeType = typeof(GenerateRepositoryAttribute<TestEntity>);

        // Act & Assert
        attributeType.Should().BeAssignableTo<Attribute>();
    }

    [Fact]
    public void GenerateRepositoryAttribute_TwoGenerics_ShouldInheritFromAttribute()
    {
        // Arrange
        var attributeType = typeof(GenerateRepositoryAttribute<TestEntity, Guid>);

        // Act & Assert
        attributeType.Should().BeAssignableTo<Attribute>();
    }

    #endregion

    #region Sealed Class Tests

    [Fact]
    public void GenerateRepositoryAttribute_SingleGeneric_ShouldBeSealed()
    {
        // Arrange
        var attributeType = typeof(GenerateRepositoryAttribute<>);

        // Act & Assert
        attributeType.IsSealed.Should().BeTrue();
    }

    [Fact]
    public void GenerateRepositoryAttribute_TwoGenerics_ShouldBeSealed()
    {
        // Arrange
        var attributeType = typeof(GenerateRepositoryAttribute<,>);

        // Act & Assert
        attributeType.IsSealed.Should().BeTrue();
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void GenerateRepositoryAttribute_SingleGeneric_ShouldHaveClassConstraint()
    {
        // Arrange
        var attributeType = typeof(GenerateRepositoryAttribute<>);
        var genericArgs = attributeType.GetGenericArguments();

        // Act & Assert
        genericArgs.Should().HaveCount(1);
        var constraint = genericArgs[0].GetGenericParameterConstraints();
        genericArgs[0].GenericParameterAttributes
            .HasFlag(System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint)
            .Should().BeTrue();
    }

    [Fact]
    public void GenerateRepositoryAttribute_TwoGenerics_EntityShouldHaveClassConstraint()
    {
        // Arrange
        var attributeType = typeof(GenerateRepositoryAttribute<,>);
        var genericArgs = attributeType.GetGenericArguments();

        // Act & Assert
        genericArgs.Should().HaveCount(2);
        // First generic parameter (TEntity) should have class constraint
        genericArgs[0].GenericParameterAttributes
            .HasFlag(System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint)
            .Should().BeTrue();
    }

    #endregion

    #region Multiple Instances Tests

    [Fact]
    public void MultipleInstances_WithDifferentEntities_ShouldBeIndependent()
    {
        // Arrange & Act
        var attribute1 = new GenerateRepositoryAttribute<TestEntity>();
        var attribute2 = new GenerateRepositoryAttribute<AnotherEntity>();

        // Assert
        attribute1.Should().NotBeNull();
        attribute2.Should().NotBeNull();
        attribute1.GetType().Should().NotBe(attribute2.GetType());
    }

    [Fact]
    public void MultipleInstances_WithDifferentIdTypes_ShouldBeIndependent()
    {
        // Arrange & Act
        var attribute1 = new GenerateRepositoryAttribute<TestEntity, Guid>();
        var attribute2 = new GenerateRepositoryAttribute<TestEntity, int>();

        // Assert
        attribute1.Should().NotBeNull();
        attribute2.Should().NotBeNull();
        attribute1.GetType().Should().NotBe(attribute2.GetType());
    }

    #endregion

    #region Generic Type Parameter Names Tests

    [Fact]
    public void GenerateRepositoryAttribute_SingleGeneric_ShouldHaveCorrectTypeParameterName()
    {
        // Arrange
        var attributeType = typeof(GenerateRepositoryAttribute<>);
        var genericArgs = attributeType.GetGenericArguments();

        // Act & Assert
        genericArgs[0].Name.Should().Be("TEntity");
    }

    [Fact]
    public void GenerateRepositoryAttribute_TwoGenerics_ShouldHaveCorrectTypeParameterNames()
    {
        // Arrange
        var attributeType = typeof(GenerateRepositoryAttribute<,>);
        var genericArgs = attributeType.GetGenericArguments();

        // Act & Assert
        genericArgs[0].Name.Should().Be("TEntity");
        genericArgs[1].Name.Should().Be("TId");
    }

    #endregion
}
