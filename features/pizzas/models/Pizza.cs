using webapi.common.domain;
using webapi.features.ingredients.models;

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

    public void AddIngredient(Ingredient ingredient)
    {
        
        _ingredients.Add(ingredient);
    }

    public void RemoveIngredient(Ingredient ingredient)
    {      

        _ingredients.Remove(ingredient);
    }

    public void Update(string name, string description, string url)
    {

        //pizza:update
        Name = name;
        Description = description;
        Url = url;
    }

    public static Pizza Create(Guid id, string name, string description, string url)
    {
        var pizza = new Pizza(id, name, description, url);
        //create pizza:create
        return pizza;
    }
}

