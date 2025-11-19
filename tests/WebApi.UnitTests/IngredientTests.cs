using webapi.features.ingredients.models;
using FluentAssertions;

namespace WebApi.UnitTests;

public class IngredientTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Tomate";
        var cost = 2.50m;

        // Act
        var result = Ingredient.Create(id, name, cost);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(id);
        result.Value.Name.Should().Be(name);
        result.Value.Cost.Should().Be(cost);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullName_ShouldReturnFailureResult(string invalidName)
    {
        // Arrange
        var id = Guid.NewGuid();
        var cost = 2.50m;

        // Act
        var result = Ingredient.Create(id, invalidName, cost);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre es requerido");
    }

    [Fact]
    public void Create_WithNameExceeding100Characters_ShouldReturnFailureResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = new string('a', 101);
        var cost = 2.50m;

        // Act
        var result = Ingredient.Create(id, name, cost);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre no puede exceder de 100 caracteres");
    }

    [Fact]
    public void Create_WithNameExactly100Characters_ShouldReturnSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = new string('a', 100);
        var cost = 2.50m;

        // Act
        var result = Ingredient.Create(id, name, cost);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Create_WithCostLessThanOrEqualToZero_ShouldReturnFailureResult(decimal invalidCost)
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Tomate";

        // Act
        var result = Ingredient.Create(id, name, invalidCost);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El costo debe ser mayor que 0");
    }

    [Theory]
    [InlineData(10000)]
    [InlineData(10000.01)]
    [InlineData(99999)]
    public void Create_WithCostGreaterThanOrEqualTo10000_ShouldReturnFailureResult(decimal invalidCost)
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Tomate";

        // Act
        var result = Ingredient.Create(id, name, invalidCost);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El costo debe ser menor que 10000");
    }

    [Fact]
    public void Create_WithCostExactly9999_99_ShouldReturnSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Tomate";
        var cost = 9999.99m;

        // Act
        var result = Ingredient.Create(id, name, cost);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithMultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "";
        var cost = -5m;

        // Act
        var result = Ingredient.Create(id, name, cost);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCountGreaterThan(1);
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre es requerido");
        result.Errors.Should().Contain(e => e.ErrorMessage == "El costo debe ser mayor que 0");
    }

    [Fact]
    public void Create_WithValidationErrors_ShouldReturnNullValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "";
        var cost = 2.50m;

        // Act
        var result = Ingredient.Create(id, name, cost);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdatePropertiesAndReturnSuccess()
    {
        // Arrange
        var createResult = Ingredient.Create(Guid.NewGuid(), "Tomate", 2.50m);
        createResult.IsSuccess.Should().BeTrue();
        var ingredient = createResult.Value!;
        var newName = "Cebolla";
        var newCost = 3.75m;

        // Act
        var result = ingredient.Update(newName, newCost);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        ingredient.Name.Should().Be(newName);
        ingredient.Cost.Should().Be(newCost);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_WithEmptyOrNullName_ShouldNotUpdateAndReturnFailure(string invalidName)
    {
        // Arrange
        var originalName = "Tomate";
        var originalCost = 2.50m;
        var createResult = Ingredient.Create(Guid.NewGuid(), originalName, originalCost);
        createResult.IsSuccess.Should().BeTrue();
        var ingredient = createResult.Value!;

        // Act
        var result = ingredient.Update(invalidName, 3.75m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre es requerido");
        ingredient.Name.Should().Be(originalName);
        ingredient.Cost.Should().Be(originalCost);
    }

    [Fact]
    public void Update_WithNameExceeding100Characters_ShouldNotUpdateAndReturnFailure()
    {
        // Arrange
        var originalName = "Tomate";
        var originalCost = 2.50m;
        var createResult = Ingredient.Create(Guid.NewGuid(), originalName, originalCost);
        createResult.IsSuccess.Should().BeTrue();
        var ingredient = createResult.Value!;
        var invalidName = new string('a', 101);

        // Act
        var result = ingredient.Update(invalidName, 3.75m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre no puede exceder de 100 caracteres");
        ingredient.Name.Should().Be(originalName);
        ingredient.Cost.Should().Be(originalCost);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Update_WithCostLessThanOrEqualToZero_ShouldNotUpdateAndReturnFailure(decimal invalidCost)
    {
        // Arrange
        var originalName = "Tomate";
        var originalCost = 2.50m;
        var createResult = Ingredient.Create(Guid.NewGuid(), originalName, originalCost);
        createResult.IsSuccess.Should().BeTrue();
        var ingredient = createResult.Value!;

        // Act
        var result = ingredient.Update("Cebolla", invalidCost);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El costo debe ser mayor que 0");
        ingredient.Name.Should().Be(originalName);
        ingredient.Cost.Should().Be(originalCost);
    }

    [Theory]
    [InlineData(10000)]
    [InlineData(10000.01)]
    public void Update_WithCostGreaterThanOrEqualTo10000_ShouldNotUpdateAndReturnFailure(decimal invalidCost)
    {
        // Arrange
        var originalName = "Tomate";
        var originalCost = 2.50m;
        var createResult = Ingredient.Create(Guid.NewGuid(), originalName, originalCost);
        createResult.IsSuccess.Should().BeTrue();
        var ingredient = createResult.Value!;

        // Act
        var result = ingredient.Update("Cebolla", invalidCost);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El costo debe ser menor que 10000");
        ingredient.Name.Should().Be(originalName);
        ingredient.Cost.Should().Be(originalCost);
    }

    [Fact]
    public void Update_WithMultipleValidationErrors_ShouldNotUpdateAndReturnAllErrors()
    {
        // Arrange
        var originalName = "Tomate";
        var originalCost = 2.50m;
        var createResult = Ingredient.Create(Guid.NewGuid(), originalName, originalCost);
        createResult.IsSuccess.Should().BeTrue();
        var ingredient = createResult.Value!;

        // Act
        var result = ingredient.Update("", -5m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCountGreaterThan(1);
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre es requerido");
        result.Errors.Should().Contain(e => e.ErrorMessage == "El costo debe ser mayor que 0");
        ingredient.Name.Should().Be(originalName);
        ingredient.Cost.Should().Be(originalCost);
    }

    [Fact]
    public void Update_ShouldNotChangeId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createResult = Ingredient.Create(id, "Tomate", 2.50m);
        createResult.IsSuccess.Should().BeTrue();
        var ingredient = createResult.Value!;

        // Act
        ingredient.Update("Cebolla", 3.75m);

        // Assert
        ingredient.Id.Should().Be(id);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_WithMinimumValidCost_ShouldReturnSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Ingrediente";
        var cost = 0.01m;

        // Act
        var result = Ingredient.Create(id, name, cost);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Cost.Should().Be(cost);
    }

    [Fact]
    public void Create_WithMaximumValidCost_ShouldReturnSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Ingrediente";
        var cost = 9999.99m;

        // Act
        var result = Ingredient.Create(id, name, cost);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Cost.Should().Be(cost);
    }

    [Fact]
    public void Update_WithNameExactly100Characters_ShouldUpdateSuccessfully()
    {
        // Arrange
        var createResult = Ingredient.Create(Guid.NewGuid(), "Tomate", 2.50m);
        createResult.IsSuccess.Should().BeTrue();
        var ingredient = createResult.Value!;
        var newName = new string('a', 100);

        // Act
        var result = ingredient.Update(newName, 2.50m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ingredient.Name.Should().Be(newName);
    }

    [Fact]
    public void Update_WithMinimumValidCost_ShouldUpdateSuccessfully()
    {
        // Arrange
        var createResult = Ingredient.Create(Guid.NewGuid(), "Tomate", 2.50m);
        createResult.IsSuccess.Should().BeTrue();
        var ingredient = createResult.Value!;
        var newCost = 0.01m;

        // Act
        var result = ingredient.Update("Tomate", newCost);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ingredient.Cost.Should().Be(newCost);
    }

    [Fact]
    public void Update_WithMaximumValidCost_ShouldUpdateSuccessfully()
    {
        // Arrange
        var createResult = Ingredient.Create(Guid.NewGuid(), "Tomate", 2.50m);
        createResult.IsSuccess.Should().BeTrue();
        var ingredient = createResult.Value!;
        var newCost = 9999.99m;

        // Act
        var result = ingredient.Update("Tomate", newCost);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ingredient.Cost.Should().Be(newCost);
    }

    #endregion

    #region ValidationError Property Tests

    [Fact]
    public void Create_WithInvalidName_ShouldReturnErrorWithPropertyName()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "";
        var cost = 2.50m;

        // Act
        var result = Ingredient.Create(id, name, cost);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = result.Errors.First(e => e.ErrorMessage == "El nombre es requerido");
        error.PropertyName.Should().Be("Name");
    }

    [Fact]
    public void Create_WithInvalidCost_ShouldReturnErrorWithPropertyName()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Tomate";
        var cost = -5m;

        // Act
        var result = Ingredient.Create(id, name, cost);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = result.Errors.First(e => e.ErrorMessage == "El costo debe ser mayor que 0");
        error.PropertyName.Should().Be("Cost");
    }

    [Fact]
    public void Update_WithInvalidName_ShouldReturnErrorWithPropertyName()
    {
        // Arrange
        var createResult = Ingredient.Create(Guid.NewGuid(), "Tomate", 2.50m);
        createResult.IsSuccess.Should().BeTrue();
        var ingredient = createResult.Value!;

        // Act
        var result = ingredient.Update("", 2.50m);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = result.Errors.First(e => e.ErrorMessage == "El nombre es requerido");
        error.PropertyName.Should().Be("Name");
    }

    [Fact]
    public void Update_WithInvalidCost_ShouldReturnErrorWithPropertyName()
    {
        // Arrange
        var createResult = Ingredient.Create(Guid.NewGuid(), "Tomate", 2.50m);
        createResult.IsSuccess.Should().BeTrue();
        var ingredient = createResult.Value!;

        // Act
        var result = ingredient.Update("Tomate", 15000m);

        // Assert
        result.IsFailure.Should().BeTrue();
        var error = result.Errors.First(e => e.ErrorMessage == "El costo debe ser menor que 10000");
        error.PropertyName.Should().Be("Cost");
    }

    #endregion
}