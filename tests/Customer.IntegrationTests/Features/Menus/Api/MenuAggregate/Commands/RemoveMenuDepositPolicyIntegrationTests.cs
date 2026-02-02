namespace Customer.IntegrationTests.Features.Menus.Api.MenuAggregate.Commands;

public class RemoveMenuDepositPolicyIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task RemoveDepositPolicy_WithExistingPolicy_Returns204()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Fianza");
        var setRequest = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PerPerson,
            Amount: 15.00m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: 6
        );
        await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", setRequest);

        // Act
        var response = await Client.DeleteAsync($"/menus/{menu.Id}/deposit-policy");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveDepositPolicy_WithExistingPolicy_ShouldRemovePolicy()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Fianza a Eliminar");
        var setRequest = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.FixedAmount,
            Amount: 50.00m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: null
        );
        await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", setRequest);

        // Act
        await Client.DeleteAsync($"/menus/{menu.Id}/deposit-policy");

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.DepositPolicy.Should().BeNull();
    }

    [Fact]
    public async Task RemoveDepositPolicy_WithNoPolicy_Returns204()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta sin Fianza");

        // Act
        var response = await Client.DeleteAsync($"/menus/{menu.Id}/deposit-policy");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveDepositPolicy_ShouldPreserveOtherMenuProperties()
    {
        // Arrange
        var menu = await CreateMenuAsync(
            name: "Carta Especial",
            description: "Descripción de la carta"
        );
        var setRequest = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PerPerson,
            Amount: 20.00m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: null
        );
        await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", setRequest);

        // Act
        await Client.DeleteAsync($"/menus/{menu.Id}/deposit-policy");

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.Name.Should().Be("Carta Especial");
        result.Description.Should().Be("Descripción de la carta");
        result.DepositPolicy.Should().BeNull();
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task RemoveDepositPolicy_WithNonExistentMenuId_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/menus/{nonExistentId}/deposit-policy");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
