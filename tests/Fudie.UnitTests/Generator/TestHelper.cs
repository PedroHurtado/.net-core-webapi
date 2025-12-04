using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Fudie.UnitTests.Generator;

/// <summary>
/// Helper para crear compilaciones en memoria con entidades de prueba
/// </summary>
public static class TestHelper
{
    /// <summary>
    /// Crea una compilación con entidades de dominio predefinidas para testing
    /// </summary>
    public static (Compilation compilation, INamedTypeSymbol customerSymbol, INamedTypeSymbol orderSymbol, INamedTypeSymbol orderItemSymbol, INamedTypeSymbol productSymbol) CreateTestCompilation()
    {
        var sourceCode = @"
using System;
using System.Collections.Generic;

namespace TestDomain
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        
        // Navegaciones
        public Address Address { get; set; }
        public ICollection<Order> Orders { get; set; }
        public List<Payment> Payments { get; set; }
        public HashSet<Notification> Notifications { get; set; }
    }

    public class Address
    {
        public Guid Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class Order
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Navegaciones
        public Customer Customer { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public Shipment Shipment { get; set; }
    }

    public class OrderItem
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        
        // Navegaciones
        public Order Order { get; set; }
        public Product Product { get; set; }
    }

    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        
        // Navegaciones
        public Category Category { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }

    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class Review
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

    public class Shipment
    {
        public Guid Id { get; set; }
        public DateTime ShipDate { get; set; }
        public string TrackingNumber { get; set; }
    }

    public class Payment
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    public class Notification
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public DateTime SentDate { get; set; }
    }
}
";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var customerSymbol = compilation.GetTypeByMetadataName("TestDomain.Customer")!;
        var orderSymbol = compilation.GetTypeByMetadataName("TestDomain.Order")!;
        var orderItemSymbol = compilation.GetTypeByMetadataName("TestDomain.OrderItem")!;
        var productSymbol = compilation.GetTypeByMetadataName("TestDomain.Product")!;

        return (compilation, customerSymbol, orderSymbol, orderItemSymbol, productSymbol);
    }

    /// <summary>
    /// Crea una compilación simple con una sola entidad para tests específicos
    /// </summary>
    public static (Compilation compilation, INamedTypeSymbol entitySymbol) CreateSimpleCompilation(string entityCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(entityCode);
        
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var entitySymbol = compilation.GetTypeByMetadataName("TestDomain.Entity")!;

        return (compilation, entitySymbol);
    }
    /// <summary>
    /// Wrapper para los datos de la compilación de prueba
    /// </summary>
    public record TestCompilationData(
        Compilation Compilation,
        INamedTypeSymbol CustomerSymbol,
        INamedTypeSymbol OrderSymbol,
        INamedTypeSymbol OrderItemSymbol,
        INamedTypeSymbol ProductSymbol
    );
}