using Fudie.Common.Domain.Exceptions;

namespace Plans.UnitTests.Features.Plans.Api.Commands.Plan;

public class UpdateProviderConfigurationTests
{
    private readonly Mock<global::Plans.Features.Plans.Api.PlanAggregate.Commands.UpdateProviderConfiguration.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly global::Plans.Features.Plans.Api.PlanAggregate.Commands.UpdateProviderConfiguration.Service _service;
    private readonly Mock<PaymentProviderConfig.Create> _providerConfigCreate;

    public UpdateProviderConfigurationTests()
    {
        var planValidator = new PlanValidator();
        _providerConfigCreate = new Mock<PaymentProviderConfig.Create>(new PaymentProviderConfigValidator());
        
        var providerConfigValidator = new PaymentProviderConfigValidator();
        var providerConfigCreate = new PaymentProviderConfig.Create(providerConfigValidator);
        
        var updateProviderConfig = new global::Plans.Features.Plans.Domain.PlanAggregate.Plan.UpdateProviderConfiguration(
            providerConfigCreate,
            planValidator
        );

        _service = new global::Plans.Features.Plans.Api.PlanAggregate.Commands.UpdateProviderConfiguration.Service(
            updateProviderConfig,
            _repository.Object,
            _unitOfWork.Object
        );
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var provider = "Stripe";
        var request = new global::Plans.Features.Plans.Api.PlanAggregate.Commands.UpdateProviderConfiguration.Request("prod_123", "price_123");

        var plan = global::Plans.Features.Plans.Domain.PlanAggregate.Plan.Create(
            new CreatePlanCommand("Basic", "Desc", 10, "USD", BillingPeriod.Monthly),
            new PlanValidator()
        );
        
        // Add a provider config to the plan so we can update it
        var providerConfigCreate = new PaymentProviderConfig.Create(new PaymentProviderConfigValidator());
        var config = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(provider, "old_prod", "old_price", true));
        
        // Using reflection to add the config to the plan for test setup
        var configsField = typeof(global::Plans.Features.Plans.Domain.PlanAggregate.Plan).GetField("_providerConfigurations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var configs = (List<PaymentProviderConfig>)configsField!.GetValue(plan)!;
        configs.Add(config);

        _repository.Setup(r => r.Get(planId)).ReturnsAsync(plan);

        // Act
        await _service.HandleAsync(planId, provider, request);

        // Assert
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        
        var updatedConfig = plan.ProviderConfigurations.First(c => c.Provider == provider);
        updatedConfig.ExternalProductId.Should().Be(request.ExternalProductId);
        updatedConfig.ExternalPriceId.Should().Be(request.ExternalPriceId);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidData_ThrowsValidationException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var provider = "Stripe";
        var request = new global::Plans.Features.Plans.Api.PlanAggregate.Commands.UpdateProviderConfiguration.Request("", ""); // Invalid data

        var plan = global::Plans.Features.Plans.Domain.PlanAggregate.Plan.Create(
            new CreatePlanCommand("Basic", "Desc", 10, "USD", BillingPeriod.Monthly),
            new PlanValidator()
        );
        
        // Add config
        var providerConfigCreate = new PaymentProviderConfig.Create(new PaymentProviderConfigValidator());
        var config = providerConfigCreate.Execute(new CreatePaymentProviderConfigCommand(provider, "old_prod", "old_price", true));
        var configsField = typeof(global::Plans.Features.Plans.Domain.PlanAggregate.Plan).GetField("_providerConfigurations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var configs = (List<PaymentProviderConfig>)configsField!.GetValue(plan)!;
        configs.Add(config);

        _repository.Setup(r => r.Get(planId)).ReturnsAsync(plan);

        // Act
        var act = () => _service.HandleAsync(planId, provider, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var provider = "NonExistent";
        var request = new global::Plans.Features.Plans.Api.PlanAggregate.Commands.UpdateProviderConfiguration.Request("prod_123", "price_123");

        var plan = global::Plans.Features.Plans.Domain.PlanAggregate.Plan.Create(
            new CreatePlanCommand("Basic", "Desc", 10, "USD", BillingPeriod.Monthly),
            new PlanValidator()
        );

        _repository.Setup(r => r.Get(planId)).ReturnsAsync(plan);

        // Act
        var act = () => _service.HandleAsync(planId, provider, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Configuration for '{provider}' not found*");
    }
}
