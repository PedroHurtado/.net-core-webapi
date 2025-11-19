using webapi.common.domain;
using webapi.features.ingredients.models;
using webapi.common;
using FluentValidation;


namespace webapi.features.pizzas.models;

public class Pizza : Entity
{
    private const decimal PROFIT = 1.20m;
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public string Url { get; protected set; }

    public decimal Price => _ingredients.Sum(i => i.Cost) * PROFIT;

    public IReadOnlyCollection<Ingredient> Ingredients => _ingredients.ToList().AsReadOnly();

    protected HashSet<Ingredient> _ingredients = [];

    protected Pizza(Guid id, string name, string description, string url) : base(id)
    {
        Name = name;
        Description = description;
        Url = url;
    }

    private Result ValidateIngredientNotNull(Ingredient ingredient)
    {
        if (ingredient == null)
        {
            return Result.Failure("El ingrediente no puede ser nulo", "Ingredient");
        }

        return Result.Success();
    }

    public Result AddIngredient(Ingredient ingredient)
    {
        var validationResult = ValidateIngredientNotNull(ingredient);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        if (_ingredients.Contains(ingredient))
        {
            return Result.Failure("El ingrediente ya existe en la pizza", "Ingredient");
        }

        _ingredients.Add(ingredient);
        return Result.Success();
    }

    public Result RemoveIngredient(Ingredient ingredient)
    {
        var validationResult = ValidateIngredientNotNull(ingredient);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        if (!_ingredients.Contains(ingredient))
        {
            return Result.Failure("El ingrediente no existe en la pizza", "Ingredient");
        }

        _ingredients.Remove(ingredient);
        return Result.Success();
    }

    public Result Update(string name, string description, string url)
    {
        var tempPizza = new Pizza(Id, name, description, url);
        var validationResult = ValidateEntity(tempPizza, new PizzaValidator());

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        Name = name;
        Description = description;
        Url = url;

        return Result.Success();
    }

    public static Result<Pizza> Create(Guid id, string name, string description, string url)
    {
        var pizza = new Pizza(id, name, description, url);
        var validationResult = ValidateEntity(pizza, new PizzaValidator());

        if (validationResult.IsFailure)
        {
            return Result<Pizza>.Failure(validationResult.Errors);
        }

        return Result<Pizza>.Success(pizza);
    }

    protected class PizzaValidator : AbstractValidator<Pizza>
    {
        public PizzaValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("El nombre es requerido")
                .MaximumLength(100)
                .WithMessage("El nombre no puede exceder de 100 caracteres");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("La descripción es requerida")
                .MaximumLength(250)
                .WithMessage("La descripción no puede exceder de 250 caracteres");

            RuleFor(x => x.Url)
                .NotEmpty()
                .WithMessage("La URL es requerida")
                .MaximumLength(500)
                .WithMessage("La URL no puede exceder de 500 caracteres")
                .Must(BeAValidUrl)
                .WithMessage("La URL no es válida");
        }

        private bool BeAValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}