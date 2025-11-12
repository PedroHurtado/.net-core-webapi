namespace webapi.features.ingredients.models;

using webapi.common.domain;
public class Ingredient:Entity
{
    
    public string Name { get; protected set; }
    public decimal Cost { get; protected set; }

    protected Ingredient(Guid id, string name, decimal cost):base(id)
    {
        //bb.dd
        //usuario no puede hacer new
        
        Name = name;
        Cost = cost;
    }
    public void Update(string name, decimal cost)
    {
        //eventos de dominio ingredient:update
        Name = name;
        Cost = cost;
    }

    public static Ingredient Create(Guid id, string name, decimal cost)
    {        
        //mocks 
        //Guid.NewGuid()
        //eventos de dominio ingredient:create
        return new Ingredient(id, name, cost);
    }

}