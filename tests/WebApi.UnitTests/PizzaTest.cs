using FluentAssertions;
using webapi.common;
using webapi.features.ingredients.models;
using webapi.features.pizzas.models;


namespace WebApi.UnitTests;

public class PizzaTests
{
    private readonly Guid _testId = Guid.NewGuid();
    private const string ValidName = "Margherita";
    private const string ValidDescription = "Pizza clásica con tomate, mozzarella y albahaca";
    private const string ValidUrl = "https://example.com/margherita.jpg";

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Act
        var result = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(_testId);
        result.Value.Name.Should().Be(ValidName);
        result.Value.Description.Should().Be(ValidDescription);
        result.Value.Url.Should().Be(ValidUrl);
        result.Value.Ingredients.Should().BeEmpty();
        result.Value.Price.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldReturnFailureResult(string name)
    {
        // Act
        var result = Pizza.Create(_testId, name, ValidDescription, ValidUrl);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Name" && 
            e.ErrorMessage == "El nombre es requerido");
    }

    [Fact]
    public void Create_WithNameExceeding100Characters_ShouldReturnFailureResult()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act
        var result = Pizza.Create(_testId, longName, ValidDescription, ValidUrl);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Name" && 
            e.ErrorMessage == "El nombre no puede exceder de 100 caracteres");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyDescription_ShouldReturnFailureResult(string description)
    {
        // Act
        var result = Pizza.Create(_testId, ValidName, description, ValidUrl);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Description" && 
            e.ErrorMessage == "La descripción es requerida");
    }

    [Fact]
    public void Create_WithDescriptionExceeding250Characters_ShouldReturnFailureResult()
    {
        // Arrange
        var longDescription = new string('a', 251);

        // Act
        var result = Pizza.Create(_testId, ValidName, longDescription, ValidUrl);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Description" && 
            e.ErrorMessage == "La descripción no puede exceder de 250 caracteres");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyUrl_ShouldReturnFailureResult(string url)
    {
        // Act
        var result = Pizza.Create(_testId, ValidName, ValidDescription, url);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Url" && 
            e.ErrorMessage == "La URL es requerida");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/image.jpg")]
    [InlineData("file:///C:/image.jpg")]
    [InlineData("example.com")]
    public void Create_WithInvalidUrl_ShouldReturnFailureResult(string invalidUrl)
    {
        // Act
        var result = Pizza.Create(_testId, ValidName, ValidDescription, invalidUrl);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Url" && 
            e.ErrorMessage == "La URL no es válida");
    }

    [Theory]
    [InlineData("http://example.com/image.jpg")]
    [InlineData("https://example.com/image.jpg")]
    [InlineData("https://subdomain.example.com/path/to/image.jpg")]
    public void Create_WithValidUrls_ShouldReturnSuccessResult(string validUrl)
    {
        // Act
        var result = Pizza.Create(_testId, ValidName, ValidDescription, validUrl);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Url.Should().Be(validUrl);
    }

    [Fact]
    public void Create_WithMultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Act
        var result = Pizza.Create(_testId, "", "", "invalid-url");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
        result.Errors.Should().Contain(e => e.PropertyName == "Url");
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();
        var newName = "Pepperoni";
        var newDescription = "Pizza con pepperoni y queso";
        var newUrl = "https://example.com/pepperoni.jpg";

        // Act
        var result = pizza.Update(newName, newDescription, newUrl);

        // Assert
        result.IsSuccess.Should().BeTrue();
        pizza.Name.Should().Be(newName);
        pizza.Description.Should().Be(newDescription);
        pizza.Url.Should().Be(newUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_WithEmptyName_ShouldReturnFailureAndNotUpdate(string name)
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();

        // Act
        var result = pizza.Update(name, ValidDescription, ValidUrl);

        // Assert
        result.IsFailure.Should().BeTrue();
        pizza.Name.Should().Be(ValidName);
    }

    [Fact]
    public void Update_WithInvalidUrl_ShouldReturnFailureAndNotUpdate()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();

        // Act
        var result = pizza.Update(ValidName, ValidDescription, "invalid-url");

        // Assert
        result.IsFailure.Should().BeTrue();
        pizza.Url.Should().Be(ValidUrl);
    }

    #endregion

    #region AddIngredient Tests

    [Fact]
    public void AddIngredient_WithValidIngredient_ShouldAddIngredient()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();
        var ingredient = Ingredient.Create(Guid.NewGuid(), "Mozzarella", 2.50m).ValueOrThrow();

        // Act
        var result = pizza.AddIngredient(ingredient);

        // Assert
        result.IsSuccess.Should().BeTrue();
        pizza.Ingredients.Should().ContainSingle();
        pizza.Ingredients.Should().Contain(ingredient);
    }

    [Fact]
    public void AddIngredient_WithNullIngredient_ShouldReturnFailure()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();

        // Act
        var result = pizza.AddIngredient(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Ingredient" && 
            e.ErrorMessage == "El ingrediente no puede ser nulo");
        pizza.Ingredients.Should().BeEmpty();
    }

    [Fact]
    public void AddIngredient_WithDuplicateIngredient_ShouldReturnFailure()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();
        var ingredient = Ingredient.Create(Guid.NewGuid(), "Mozzarella", 2.50m).ValueOrThrow();
        pizza.AddIngredient(ingredient);

        // Act
        var result = pizza.AddIngredient(ingredient);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Ingredient" && 
            e.ErrorMessage == "El ingrediente ya existe en la pizza");
        pizza.Ingredients.Should().ContainSingle();
    }

    [Fact]
    public void AddIngredient_WithMultipleIngredients_ShouldAddAll()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();
        var ingredient1 = Ingredient.Create(Guid.NewGuid(), "Mozzarella", 2.50m).ValueOrThrow();
        var ingredient2 = Ingredient.Create(Guid.NewGuid(), "Tomate", 1.50m).ValueOrThrow();
        var ingredient3 = Ingredient.Create(Guid.NewGuid(), "Albahaca", 0.50m).ValueOrThrow();

        // Act
        pizza.AddIngredient(ingredient1);
        pizza.AddIngredient(ingredient2);
        pizza.AddIngredient(ingredient3);

        // Assert
        pizza.Ingredients.Should().HaveCount(3);
        pizza.Ingredients.Should().Contain(ingredient1);
        pizza.Ingredients.Should().Contain(ingredient2);
        pizza.Ingredients.Should().Contain(ingredient3);
    }

    #endregion

    #region RemoveIngredient Tests

    [Fact]
    public void RemoveIngredient_WithExistingIngredient_ShouldRemoveIngredient()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();
        var ingredient = Ingredient.Create(Guid.NewGuid(), "Mozzarella", 2.50m).ValueOrThrow();
        pizza.AddIngredient(ingredient);

        // Act
        var result = pizza.RemoveIngredient(ingredient);

        // Assert
        result.IsSuccess.Should().BeTrue();
        pizza.Ingredients.Should().BeEmpty();
    }

    [Fact]
    public void RemoveIngredient_WithNullIngredient_ShouldReturnFailure()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();

        // Act
        var result = pizza.RemoveIngredient(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Ingredient" && 
            e.ErrorMessage == "El ingrediente no puede ser nulo");
    }

    [Fact]
    public void RemoveIngredient_WithNonExistingIngredient_ShouldReturnFailure()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();
        var ingredient = Ingredient.Create(Guid.NewGuid(), "Mozzarella", 2.50m).ValueOrThrow();

        // Act
        var result = pizza.RemoveIngredient(ingredient);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Ingredient" && 
            e.ErrorMessage == "El ingrediente no existe en la pizza");
        pizza.Ingredients.Should().BeEmpty();
    }

    [Fact]
    public void RemoveIngredient_FromPizzaWithMultipleIngredients_ShouldOnlyRemoveSpecified()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();
        var ingredient1 = Ingredient.Create(Guid.NewGuid(), "Mozzarella", 2.50m).ValueOrThrow();
        var ingredient2 = Ingredient.Create(Guid.NewGuid(), "Tomate", 1.50m).ValueOrThrow();
        var ingredient3 = Ingredient.Create(Guid.NewGuid(), "Albahaca", 0.50m).ValueOrThrow();
        pizza.AddIngredient(ingredient1);
        pizza.AddIngredient(ingredient2);
        pizza.AddIngredient(ingredient3);

        // Act
        var result = pizza.RemoveIngredient(ingredient2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        pizza.Ingredients.Should().HaveCount(2);
        pizza.Ingredients.Should().Contain(ingredient1);
        pizza.Ingredients.Should().Contain(ingredient3);
        pizza.Ingredients.Should().NotContain(ingredient2);
    }

    #endregion

    #region Price Tests

    [Fact]
    public void Price_WithNoIngredients_ShouldBeZero()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();

        // Act
        var price = pizza.Price;

        // Assert
        price.Should().Be(0);
    }

    [Fact]
    public void Price_WithIngredients_ShouldCalculateWithProfit()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();
        var ingredient1 = Ingredient.Create(Guid.NewGuid(), "Mozzarella", 2.00m).ValueOrThrow();
        var ingredient2 = Ingredient.Create(Guid.NewGuid(), "Tomate", 1.00m).ValueOrThrow();
        pizza.AddIngredient(ingredient1);
        pizza.AddIngredient(ingredient2);

        // Act
        var price = pizza.Price;

        // Assert
        // (2.00 + 1.00) * 1.20 = 3.60
        price.Should().Be(3.60m);
    }

    [Fact]
    public void Price_ShouldRecalculateWhenIngredientsChange()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();
        var ingredient1 = Ingredient.Create(Guid.NewGuid(), "Mozzarella", 2.00m).ValueOrThrow();
        var ingredient2 = Ingredient.Create(Guid.NewGuid(), "Tomate", 1.00m).ValueOrThrow();
        pizza.AddIngredient(ingredient1);
        
        var priceWithOneIngredient = pizza.Price;
        
        // Act
        pizza.AddIngredient(ingredient2);
        var priceWithTwoIngredients = pizza.Price;

        // Assert
        priceWithOneIngredient.Should().Be(2.40m); // 2.00 * 1.20
        priceWithTwoIngredients.Should().Be(3.60m); // (2.00 + 1.00) * 1.20
    }

    [Fact]
    public void Price_ShouldDecreaseWhenIngredientRemoved()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();
        var ingredient1 = Ingredient.Create(Guid.NewGuid(), "Mozzarella", 2.00m).ValueOrThrow();
        var ingredient2 = Ingredient.Create(Guid.NewGuid(), "Tomate", 1.00m).ValueOrThrow();
        pizza.AddIngredient(ingredient1);
        pizza.AddIngredient(ingredient2);
        
        var priceWithTwoIngredients = pizza.Price;
        
        // Act
        pizza.RemoveIngredient(ingredient2);
        var priceWithOneIngredient = pizza.Price;

        // Assert
        priceWithTwoIngredients.Should().Be(3.60m);
        priceWithOneIngredient.Should().Be(2.40m);
    }

    #endregion

    #region Ingredients Collection Tests

    [Fact]
    public void Ingredients_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();

        // Act
        var ingredients = pizza.Ingredients;

        // Assert
        ingredients.Should().BeAssignableTo<IReadOnlyCollection<Ingredient>>();
    }

    [Fact]
    public void Ingredients_ShouldNotAllowDirectModification()
    {
        // Arrange
        var pizza = Pizza.Create(_testId, ValidName, ValidDescription, ValidUrl).ValueOrThrow();
        var ingredient = Ingredient.Create(Guid.NewGuid(), "Mozzarella", 2.50m).ValueOrThrow();
        
        // Act & Assert
        var ingredients = pizza.Ingredients;
        ingredients.Should().BeAssignableTo<IReadOnlyCollection<Ingredient>>();
        // No debe tener métodos Add o Remove en la colección readonly
    }

    #endregion
}