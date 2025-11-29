namespace webapi.features.ingredients.models;

using Fudie;
using Fudie.Domain;
using FluentValidation;

public class Ingredient : Entity
{
    public string Name { get; protected set; }
    public decimal Cost { get; protected set; }

    protected Ingredient(Guid id, string name, decimal cost) : base(id)
    {
        Name = name;
        Cost = cost;
    }

    public Result Update(string name, decimal cost)
    {
        var tempIngredient = new Ingredient(Id, name, cost);
        var validationResult = ValidateEntity(tempIngredient, new IngredientValidator());
        
        if (validationResult.IsFailure)
        {
            return validationResult;
        }
        
        Name = name;
        Cost = cost;
        
        return Result.Success();
    }

    public static Result<Ingredient> Create(Guid id, string name, decimal cost)
    {
        var ingredient = new Ingredient(id, name, cost);
        var validationResult = ValidateEntity(ingredient, new IngredientValidator());
        
        if (validationResult.IsFailure)
        {
            return Result<Ingredient>.Failure(validationResult.Errors);
        }
        
        return Result<Ingredient>.Success(ingredient);
    }

    protected class IngredientValidator : AbstractValidator<Ingredient>
    {
        public IngredientValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("El nombre es requerido")
                .MaximumLength(100)
                .WithMessage("El nombre no puede exceder de 100 caracteres");

            RuleFor(x => x.Cost)
                .GreaterThan(0)
                .WithMessage("El costo debe ser mayor que 0")
                .LessThan(10000)
                .WithMessage("El costo debe ser menor que 10000");
        }
    }
}