using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using webapi.features.pizzas.models;

namespace webapi.infrastructure.configurations;

public class PizzaConfiguration : IEntityTypeConfiguration<Pizza>
{
    public void Configure(EntityTypeBuilder<Pizza> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(p => p.Url)
            .IsRequired()
            .HasMaxLength(500);

        // Configuración de la propiedad calculada Price
        builder.Ignore(p => p.Price);

        // Configuración de la relación N:M con Ingredients
        builder.HasMany(p => p.Ingredients)
            .WithMany()
            .UsingEntity(j => j.ToTable("PizzaIngredients"));

        // Mapeo del campo privado _ingredients
        var navigation = builder.Metadata.FindNavigation(nameof(Pizza.Ingredients));
        navigation?.SetField("_ingredients");
    }
}