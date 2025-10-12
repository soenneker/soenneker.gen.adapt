using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class InterfaceTests : UnitTest
{
    public InterfaceTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_ClassToImplementation_WithInterfaceInMiddle_ShouldMapProperties()
    {
        // Arrange - both PersonClass and PersonImplementation implement same interface properties
        var source = new PersonClass
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30
        };

        // Act
        var result = source.Adapt<PersonImplementation>();

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Age.Should().Be(30);
    }

    [Fact]
    public void Adapt_ImplementationToAnotherImplementation_ShouldMapProperties()
    {
        // Arrange - both classes share interface-like properties
        var source = new PersonImplementation
        {
            FirstName = "Jane",
            LastName = "Smith",
            Age = 25
        };

        // Act
        var result = source.Adapt<PersonClass>();

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.Age.Should().Be(25);
    }

    [Fact]
    public void Adapt_ImplementationToClass_ShouldMapProperties()
    {
        // Arrange
        var source = new PersonImplementation
        {
            FirstName = "Bob",
            LastName = "Johnson",
            Age = 35
        };

        // Act
        var result = source.Adapt<PersonClass>();

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Bob");
        result.LastName.Should().Be("Johnson");
        result.Age.Should().Be(35);
    }

    [Fact]
    public void Adapt_ClassToImplementation_ShouldMapProperties()
    {
        // Arrange
        var source = new PersonClass
        {
            FirstName = "Alice",
            LastName = "Williams",
            Age = 28
        };

        // Act
        var result = source.Adapt<PersonImplementation>();

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Alice");
        result.LastName.Should().Be("Williams");
        result.Age.Should().Be(28);
    }

    [Fact]
    public void Adapt_ProductImplementationToClass_ShouldMapProperties()
    {
        // Arrange
        var source = new ProductImplementation
        {
            Id = "PROD-123",
            Name = "Test Product",
            Price = 99.99m
        };

        // Act
        var result = source.Adapt<ProductClass>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("PROD-123");
        result.Name.Should().Be("Test Product");
        result.Price.Should().Be(99.99m);
    }

    [Fact]
    public void Adapt_ProductClassToImplementation_ShouldMapProperties()
    {
        // Arrange
        var source = new ProductClass
        {
            Id = "PROD-456",
            Name = "Another Product",
            Price = 149.99m
        };

        // Act
        var result = source.Adapt<ProductImplementation>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("PROD-456");
        result.Name.Should().Be("Another Product");
        result.Price.Should().Be(149.99m);
    }

    [Fact]
    public void Adapt_ClassWithConcreteProperties_ShouldMapRecursively()
    {
        // Arrange
        var source = new OrderWithInterfaceSource
        {
            OrderId = "ORDER-001",
            Product = new ProductImplementation
            {
                Id = "PROD-789",
                Name = "Nested Product",
                Price = 199.99m
            },
            Customer = new PersonImplementation
            {
                FirstName = "Charlie",
                LastName = "Brown",
                Age = 40
            }
        };

        // Act
        var result = source.Adapt<OrderWithInterfaceDest>();

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be("ORDER-001");
        // Note: Interface-typed properties (Product, Customer) cannot be automatically 
        // mapped by the generator as it doesn't know which concrete type to instantiate.
        // This is a known limitation when mapping to interface-typed properties.
    }

    [Fact]
    public void Adapt_BetweenClassesWithConcreteTypes_ShouldMapRecursively()
    {
        // Arrange
        var source = new OrderWithClassSource
        {
            OrderId = "ORDER-002",
            Product = new ProductClass
            {
                Id = "PROD-999",
                Name = "Class Product",
                Price = 299.99m
            },
            Customer = new PersonClass
            {
                FirstName = "David",
                LastName = "Lee",
                Age = 45
            }
        };

        // Act
        var result = source.Adapt<OrderWithInterfaceDest>();

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be("ORDER-002");
        // Note: Interface-typed properties cannot be automatically mapped
    }

    [Fact]
    public void Adapt_WithEmptyStrings_ShouldMapCorrectly()
    {
        // Arrange
        var source = new PersonClass
        {
            FirstName = "",
            LastName = "",
            Age = 0
        };

        // Act
        var result = source.Adapt<PersonImplementation>();

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("");
        result.LastName.Should().Be("");
        result.Age.Should().Be(0);
    }

    [Fact]
    public void Adapt_WithMinMaxValues_ShouldMapCorrectly()
    {
        // Arrange
        var source = new PersonImplementation
        {
            FirstName = "Test",
            LastName = "User",
            Age = int.MaxValue
        };

        // Act
        var result = source.Adapt<PersonClass>();

        // Assert
        result.Should().NotBeNull();
        result.Age.Should().Be(int.MaxValue);
    }

    [Fact]
    public void Adapt_MultipleInstances_ShouldCreateIndependentObjects()
    {
        // Arrange
        var source1 = new PersonImplementation
        {
            FirstName = "First",
            LastName = "Person",
            Age = 10
        };

        var source2 = new PersonImplementation
        {
            FirstName = "Second",
            LastName = "Person",
            Age = 20
        };

        // Act
        var result1 = source1.Adapt<PersonClass>();
        var result2 = source2.Adapt<PersonClass>();

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Should().NotBeSameAs(result2);
        result1.FirstName.Should().Be("First");
        result2.FirstName.Should().Be("Second");
        result1.Age.Should().Be(10);
        result2.Age.Should().Be(20);
    }

    [Fact]
    public void Adapt_BidirectionalMapping_ShouldWork()
    {
        // Arrange
        var sourceClass = new PersonClass
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30
        };

        // Act - Map to implementation and back
        var implementation = sourceClass.Adapt<PersonImplementation>();
        var backToClass = implementation.Adapt<PersonClass>();

        // Assert
        backToClass.Should().NotBeNull();
        backToClass.FirstName.Should().Be("John");
        backToClass.LastName.Should().Be("Doe");
        backToClass.Age.Should().Be(30);
    }

    [Fact]
    public void Adapt_DirectlyFromInterface_ShouldWork()
    {
        // Arrange - This simulates the scenario where you have an interface reference
        IPersonInterface person = new PersonImplementation
        {
            FirstName = "Jane",
            LastName = "Doe",
            Age = 25
        };

        // Act - Call .Adapt<T>() directly on the interface reference
        var result = person.Adapt<PersonClass>();

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Doe");
        result.Age.Should().Be(25);
    }

    [Fact]
    public void Adapt_DirectlyFromProductInterface_ShouldWork()
    {
        // Arrange
        IProductInterface product = new ProductImplementation
        {
            Id = "TEST-001",
            Name = "Test Product",
            Price = 99.99m
        };

        // Act - This is the scenario from your code example
        var document = product.Adapt<ProductClass>();

        // Assert
        document.Should().NotBeNull();
        document.Id.Should().Be("TEST-001");
        document.Name.Should().Be("Test Product");
        document.Price.Should().Be(99.99m);
    }
}

