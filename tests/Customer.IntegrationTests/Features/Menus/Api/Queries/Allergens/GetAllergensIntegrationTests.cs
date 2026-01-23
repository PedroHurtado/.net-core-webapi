using System.Net;
using System.Net.Http.Json;
using Customer.Features.Menus.Api.Queries.Allergens;
using Customer.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Customer.IntegrationTests.Features.Menus.Api.Queries.Allergens;

public class GetAllergensIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task GetAllergens_ShouldReturnOkAndList()
    {
        // Arrange
        var created1 = await CreateAllergenAsync(name: "Gluten Test");
        var created2 = await CreateAllergenAsync(name: "Lácteos Test");

        // Act
        var response = await Client.GetAsync("/allergens");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<GetAllergens.Response>>();
        result.Should().NotBeNull();

        // Verificar que los allergens creados están en la respuesta
        result!.Should().Contain(a => a.Id == created1.Id);
        result.Should().Contain(a => a.Id == created2.Id);
    }

    [Fact]
    public async Task GetAllergens_ShouldReturnAllFields()
    {
        // Arrange
        var created = await CreateAllergenAsync(
            name: "Nueces Test",
            iconUrl: "https://example.com/nueces.png",
            isActive: false,
            displayOrder: 5
        );

        // Act
        var response = await Client.GetAsync("/allergens");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<GetAllergens.Response>>();
        var allergen = result!.First(a => a.Id == created.Id);

        allergen.Name.Should().Be("Nueces Test");
        allergen.IconUrl.Should().Be("https://example.com/nueces.png");
        allergen.IsActive.Should().BeFalse();
        allergen.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public async Task GetAllergens_ShouldReturnOrderedByDisplayOrderThenName()
    {
        // Arrange - Usar prefijo único para este test
        var testPrefix = Guid.NewGuid().ToString("N")[..4].ToUpper();

        var created1 = await CreateAllergenAsync(code: $"Z_{testPrefix}_1", name: $"Zeta_{testPrefix}", displayOrder: 100);
        var created2 = await CreateAllergenAsync(code: $"A_{testPrefix}_2", name: $"Alfa_{testPrefix}", displayOrder: 100);
        var created3 = await CreateAllergenAsync(code: $"B_{testPrefix}_3", name: $"Beta_{testPrefix}", displayOrder: 99);

        // Act
        var response = await Client.GetAsync("/allergens");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<GetAllergens.Response>>();

        // Filtrar solo los allergens creados en este test
        var testAllergens = result!
            .Where(a => a.Id == created1.Id || a.Id == created2.Id || a.Id == created3.Id)
            .ToList();

        testAllergens.Should().HaveCount(3);

        // Beta (displayOrder: 99) debe venir antes que Alfa y Zeta (displayOrder: 100)
        var betaIndex = testAllergens.FindIndex(a => a.Id == created3.Id);
        var alfaIndex = testAllergens.FindIndex(a => a.Id == created2.Id);
        var zetaIndex = testAllergens.FindIndex(a => a.Id == created1.Id);

        betaIndex.Should().BeLessThan(alfaIndex);
        betaIndex.Should().BeLessThan(zetaIndex);

        // Con mismo displayOrder, Alfa debe venir antes que Zeta (orden alfabético)
        alfaIndex.Should().BeLessThan(zetaIndex);
    }

    #endregion
}
