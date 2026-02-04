using FluentAssertions;
using FluentValidation;
using Fudie.DependencyInjection;
using Fudie.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Fudie.UnitTests.DependencyInjection;

public class DomainCommandsExtensionTests
{
    #region AddDomainCommands Tests - Basic Functionality

    [Fact]
    public void AddDomainCommands_WithNoAssemblies_ShouldUseCallingAssembly()
    {
        var services = new ServiceCollection();

        services.AddDomainCommands();

        services.Should().NotBeNull();
    }

    [Fact]
    public void AddDomainCommands_ShouldReturnServiceCollection()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestValidator).Assembly;

        var result = services.AddDomainCommands(assembly);

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddDomainCommands_ShouldAllowChaining()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestValidator).Assembly;

        var result = services
            .AddDomainCommands(assembly)
            .AddDomainCommands(assembly);

        result.Should().BeSameAs(services);
    }

    #endregion

    #region AddDomainCommands Tests - Validators

    [Fact]
    public void AddDomainCommands_WithValidator_ShouldRegisterAsSingleton()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestValidator).Assembly;

        services.AddDomainCommands(assembly);

        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(TestValidator));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddDomainCommands_WithAbstractValidator_ShouldNotRegister()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestValidator).Assembly;

        services.AddDomainCommands(assembly);

        services.Should().NotContain(sd => sd.ServiceType == typeof(AbstractValidator<>));
    }

    #endregion

    #region AddDomainCommands Tests - CreateCommand

    [Fact]
    public void AddDomainCommands_WithCreateCommand_ShouldRegisterAsSingleton()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestCreateCommand).Assembly;

        services.AddDomainCommands(assembly);

        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(TestCreateCommand));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddDomainCommands_WithAbstractCreateCommand_ShouldNotRegister()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestCreateCommand).Assembly;

        services.AddDomainCommands(assembly);

        services.Should().NotContain(sd => sd.ServiceType == typeof(AbstractCreateCommand<,>));
    }

    #endregion

    #region AddDomainCommands Tests - ModifyCommand without data

    [Fact]
    public void AddDomainCommands_WithModifyCommandWithoutData_ShouldRegisterAsSingleton()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestModifyCommandWithoutData).Assembly;

        services.AddDomainCommands(assembly);

        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(TestModifyCommandWithoutData));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddDomainCommands_WithAbstractModifyCommandWithoutData_ShouldNotRegister()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestModifyCommandWithoutData).Assembly;

        services.AddDomainCommands(assembly);

        services.Should().NotContain(sd => sd.ServiceType == typeof(AbstractModifyCommand<>));
    }

    #endregion

    #region AddDomainCommands Tests - ModifyCommand with data

    [Fact]
    public void AddDomainCommands_WithModifyCommandWithData_ShouldRegisterAsSingleton()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestModifyCommandWithData).Assembly;

        services.AddDomainCommands(assembly);

        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(TestModifyCommandWithData));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddDomainCommands_WithAbstractModifyCommandWithData_ShouldNotRegister()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestModifyCommandWithData).Assembly;

        services.AddDomainCommands(assembly);

        services.Should().NotContain(sd => sd.ServiceType == typeof(AbstractModifyCommand<,>));
    }

    #endregion

    #region AddDomainCommands Tests - Multiple Types

    [Fact]
    public void AddDomainCommands_ShouldRegisterAllDomainTypes()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestValidator).Assembly;

        services.AddDomainCommands(assembly);

        services.Should().Contain(sd => sd.ServiceType == typeof(TestValidator));
        services.Should().Contain(sd => sd.ServiceType == typeof(TestCreateCommand));
        services.Should().Contain(sd => sd.ServiceType == typeof(TestModifyCommandWithoutData));
        services.Should().Contain(sd => sd.ServiceType == typeof(TestModifyCommandWithData));
    }

    [Fact]
    public void AddDomainCommands_ShouldNotRegisterNonDomainTypes()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestValidator).Assembly;

        services.AddDomainCommands(assembly);

        services.Should().NotContain(sd => sd.ServiceType == typeof(NonDomainClass));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_Validator_ShouldResolve()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestValidator).Assembly;
        services.AddDomainCommands(assembly);
        var provider = services.BuildServiceProvider();

        var validator = provider.GetService<TestValidator>();

        validator.Should().NotBeNull();
        validator.Should().BeOfType<TestValidator>();
    }

    [Fact]
    public void Integration_CreateCommand_ShouldResolve()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestCreateCommand).Assembly;
        services.AddDomainCommands(assembly);
        var provider = services.BuildServiceProvider();

        var command = provider.GetService<TestCreateCommand>();

        command.Should().NotBeNull();
        command.Should().BeOfType<TestCreateCommand>();
    }

    [Fact]
    public void Integration_ModifyCommandWithoutData_ShouldResolve()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestModifyCommandWithoutData).Assembly;
        services.AddDomainCommands(assembly);
        var provider = services.BuildServiceProvider();

        var command = provider.GetService<TestModifyCommandWithoutData>();

        command.Should().NotBeNull();
        command.Should().BeOfType<TestModifyCommandWithoutData>();
    }

    [Fact]
    public void Integration_ModifyCommandWithData_ShouldResolve()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestModifyCommandWithData).Assembly;
        services.AddDomainCommands(assembly);
        var provider = services.BuildServiceProvider();

        var command = provider.GetService<TestModifyCommandWithData>();

        command.Should().NotBeNull();
        command.Should().BeOfType<TestModifyCommandWithData>();
    }

    [Fact]
    public void Integration_Singleton_ShouldReturnSameInstance()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestValidator).Assembly;
        services.AddDomainCommands(assembly);
        var provider = services.BuildServiceProvider();

        var validator1 = provider.GetService<TestValidator>();
        var validator2 = provider.GetService<TestValidator>();

        validator1.Should().BeSameAs(validator2);
    }

    [Fact]
    public void Integration_CommandWithDependency_ShouldResolveWithDependencies()
    {
        var services = new ServiceCollection();
        var assembly = typeof(TestCreateCommandWithDependency).Assembly;
        services.AddDomainCommands(assembly);
        var provider = services.BuildServiceProvider();

        var command = provider.GetService<TestCreateCommandWithDependency>();

        command.Should().NotBeNull();
        command!.Validator.Should().NotBeNull();
    }

    #endregion

    #region Test Helper Classes

    private class TestEntity : IEntity
    {
        public Guid Id { get; set; }
    }

    private record TestCommand(string Value);

    private class TestValidator : AbstractValidator<TestEntity>
    {
        public TestValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    private class TestCreateCommand : AbstractCreateCommand<TestCommand, TestEntity>
    {
        public override TestEntity Execute(TestCommand command)
        {
            return new TestEntity { Id = Guid.NewGuid() };
        }
    }

    private class TestModifyCommandWithoutData : AbstractModifyCommand<TestEntity>
    {
        public override TestEntity Execute(TestEntity entity)
        {
            return entity;
        }
    }

    private class TestModifyCommandWithData : AbstractModifyCommand<TestCommand, TestEntity>
    {
        public override TestEntity Execute(TestEntity entity, TestCommand command)
        {
            return entity;
        }
    }

    private class TestCreateCommandWithDependency(TestValidator validator)
        : AbstractCreateCommand<TestCommand, TestEntity>
    {
        public TestValidator Validator { get; } = validator;

        public override TestEntity Execute(TestCommand command)
        {
            return new TestEntity { Id = Guid.NewGuid() };
        }
    }

    private class NonDomainClass
    {
        public string Name { get; set; } = "";
    }

    #endregion
}