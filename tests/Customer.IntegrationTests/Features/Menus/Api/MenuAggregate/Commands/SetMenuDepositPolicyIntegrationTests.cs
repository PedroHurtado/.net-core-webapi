namespace Customer.IntegrationTests.Features.Menus.Api.MenuAggregate.Commands;

public class SetMenuDepositPolicyIntegrationTests(WebApplicationFactory<Program> factory)
    : CustomerWebApplicationFixture(factory)
{
    #region Success Tests

    [Fact]
    public async Task SetDepositPolicy_WithPerPerson_Returns204()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Fianza");
        var request = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PerPerson,
            Amount: 15.00m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: 6
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetDepositPolicy_WithPerPerson_ShouldPersistPolicy()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Fianza PerPerson");
        var request = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PerPerson,
            Amount: 20.00m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: 4
        );

        // Act
        await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", request);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.DepositPolicy.Should().NotBeNull();
        result.DepositPolicy!.DepositType.Should().Be(DepositType.PerPerson);
        result.DepositPolicy.Amount.Should().Be(20.00m);
        result.DepositPolicy.MinimumGuestsForDeposit.Should().Be(4);
    }

    [Fact]
    public async Task SetDepositPolicy_WithPercentageOfBill_Returns204()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Porcentaje");
        var request = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PercentageOfBill,
            Amount: 10.00m,
            Percentage: 20,
            MinimumBillForDeposit: 100.00m,
            MinimumGuestsForDeposit: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetDepositPolicy_WithPercentageOfBill_ShouldPersistPolicy()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Porcentaje Persistido");
        var request = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PercentageOfBill,
            Amount: 15.00m,
            Percentage: 25,
            MinimumBillForDeposit: 150.00m,
            MinimumGuestsForDeposit: null
        );

        // Act
        await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", request);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.DepositPolicy.Should().NotBeNull();
        result.DepositPolicy!.DepositType.Should().Be(DepositType.PercentageOfBill);
        result.DepositPolicy.Percentage.Should().Be(25);
        result.DepositPolicy.MinimumBillForDeposit.Should().Be(150.00m);
    }

    [Fact]
    public async Task SetDepositPolicy_WithFixedAmount_Returns204()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Importe Fijo");
        var request = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.FixedAmount,
            Amount: 50.00m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetDepositPolicy_WithFixedAmount_ShouldPersistPolicy()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Importe Fijo Persistido");
        var request = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.FixedAmount,
            Amount: 75.00m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: null
        );

        // Act
        await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", request);

        // Assert
        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.DepositPolicy.Should().NotBeNull();
        result.DepositPolicy!.DepositType.Should().Be(DepositType.FixedAmount);
        result.DepositPolicy.Amount.Should().Be(75.00m);
    }

    [Fact]
    public async Task SetDepositPolicy_ShouldReplaceExistingPolicy()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Política Existente");
        var firstRequest = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PerPerson,
            Amount: 10.00m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: 2
        );
        await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", firstRequest);

        var secondRequest = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.FixedAmount,
            Amount: 100.00m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/menus/{menu.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<MenuResponse>(JsonOptions);

        result!.DepositPolicy!.DepositType.Should().Be(DepositType.FixedAmount);
        result.DepositPolicy.Amount.Should().Be(100.00m);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task SetDepositPolicy_WithNonExistentMenuId_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PerPerson,
            Amount: 15.00m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{nonExistentId}/deposit-policy", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task SetDepositPolicy_WithNegativeAmount_Returns422()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Amount Negativo");
        var request = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PerPerson,
            Amount: -10.00m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetDepositPolicy_WithZeroAmount_Returns422()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Amount Cero");
        var request = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PerPerson,
            Amount: 0m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetDepositPolicy_WithPercentageOfBillWithoutPercentage_Returns422()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta sin Porcentaje");
        var request = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PercentageOfBill,
            Amount: 10.00m,
            Percentage: null,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetDepositPolicy_WithInvalidPercentage_Returns422()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Porcentaje Inválido");
        var request = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PercentageOfBill,
            Amount: 10.00m,
            Percentage: 150,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetDepositPolicy_WithPercentageOnNonPercentageType_Returns422()
    {
        // Arrange
        var menu = await CreateMenuAsync(name: "Carta con Porcentaje en Tipo Incorrecto");
        var request = new SetMenuDepositPolicy.Request(
            DepositType: DepositType.PerPerson,
            Amount: 15.00m,
            Percentage: 20,
            MinimumBillForDeposit: null,
            MinimumGuestsForDeposit: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/menus/{menu.Id}/deposit-policy", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}
