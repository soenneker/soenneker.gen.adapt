using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using System.Collections.Generic;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public static class Test
{


}

public sealed class InterfaceCollectionTests : UnitTest
{
    public InterfaceCollectionTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_ListOfInterfaces_ShouldMapToListOfClasses()
    {
        // Arrange
        var source = new ListOfInterfacesSource
        {
            People = new List<IPersonInterface>
            {
                new PersonImplementation { FirstName = "John", LastName = "Doe", Age = 30 },
                new PersonImplementation { FirstName = "Jane", LastName = "Smith", Age = 25 }
            },
            Products = new List<IProductInterface>
            {
                new ProductImplementation { Id = "P1", Name = "Product 1", Price = 99.99m },
                new ProductImplementation { Id = "P2", Name = "Product 2", Price = 149.99m }
            }
        };

        // Act
        var result = source.Adapt<ListOfInterfacesDest>();

        // Assert
        result.Should().NotBeNull();
        result.People.Should().NotBeNull();
        result.People.Count.Should().Be(2);
        result.People[0].FirstName.Should().Be("John");
        result.People[0].LastName.Should().Be("Doe");
        result.People[0].Age.Should().Be(30);
        result.People[1].FirstName.Should().Be("Jane");
        result.People[1].LastName.Should().Be("Smith");
        result.People[1].Age.Should().Be(25);

        result.Products.Should().NotBeNull();
        result.Products.Count.Should().Be(2);
        result.Products[0].Id.Should().Be("P1");
        result.Products[0].Name.Should().Be("Product 1");
        result.Products[0].Price.Should().Be(99.99m);
        result.Products[1].Id.Should().Be("P2");
        result.Products[1].Name.Should().Be("Product 2");
        result.Products[1].Price.Should().Be(149.99m);
    }

    [Fact]
    public void Adapt_IReadOnlyListOfInterfaces_ShouldMapToList()
    {
        // Arrange
        var source = new ReadOnlyListSource
        {
            People = new List<IPersonInterface>
            {
                new PersonImplementation { FirstName = "Alice", LastName = "Johnson", Age = 35 },
                new PersonImplementation { FirstName = "Bob", LastName = "Williams", Age = 40 }
            },
            Products = new List<IProductInterface>
            {
                new ProductImplementation { Id = "P3", Name = "Product 3", Price = 199.99m }
            }
        };

        // Act
        var result = source.Adapt<ReadOnlyListDest>();

        // Assert
        result.Should().NotBeNull();
        result.People.Should().NotBeNull();
        result.People.Count.Should().Be(2);
        result.People[0].FirstName.Should().Be("Alice");
        result.People[0].Age.Should().Be(35);
        result.People[1].FirstName.Should().Be("Bob");
        result.People[1].Age.Should().Be(40);

        result.Products.Should().NotBeNull();
        result.Products.Count.Should().Be(1);
        result.Products[0].Id.Should().Be("P3");
        result.Products[0].Price.Should().Be(199.99m);
    }

    [Fact]
    public void Adapt_EmptyListOfInterfaces_ShouldMapToEmptyList()
    {
        // Arrange
        var source = new ListOfInterfacesSource
        {
            People = new List<IPersonInterface>(),
            Products = new List<IProductInterface>()
        };

        // Act
        var result = source.Adapt<ListOfInterfacesDest>();

        // Assert
        result.Should().NotBeNull();
        result.People.Should().NotBeNull();
        result.People.Count.Should().Be(0);
        result.Products.Should().NotBeNull();
        result.Products.Count.Should().Be(0);
    }

    [Fact]
    public void Adapt_ListOfInterfacesWithManyItems_ShouldMapAll()
    {
        // Arrange
        var people = new List<IPersonInterface>();
        for (int i = 0; i < 100; i++)
        {
            people.Add(new PersonImplementation 
            { 
                FirstName = $"First{i}", 
                LastName = $"Last{i}", 
                Age = i 
            });
        }

        var source = new ListOfInterfacesSource
        {
            People = people,
            Products = new List<IProductInterface>()
        };

        // Act
        var result = source.Adapt<ListOfInterfacesDest>();

        // Assert
        result.Should().NotBeNull();
        result.People.Count.Should().Be(100);
        result.People[50].FirstName.Should().Be("First50");
        result.People[99].Age.Should().Be(99);
    }

    [Fact]
    public void Adapt_ListOfInterfaces_ShouldCreateIndependentCopies()
    {
        // Arrange
        var person = new PersonImplementation { FirstName = "Original", LastName = "Name", Age = 30 };
        var source = new ListOfInterfacesSource
        {
            People = new List<IPersonInterface> { person },
            Products = new List<IProductInterface>()
        };

        // Act
        var result = source.Adapt<ListOfInterfacesDest>();

        // Modify original
        person.FirstName = "Modified";

        // Assert
        result.People[0].FirstName.Should().Be("Original"); // Should be unchanged
    }

    [Fact]
    public void Adapt_DictionaryWithInterfaceValues_ShouldMapValueTypes()
    {
        // Arrange
        var source = new DictWithInterfacesSource
        {
            PeopleById = new Dictionary<string, IPersonInterface>
            {
                ["person1"] = new PersonImplementation { FirstName = "John", LastName = "Doe", Age = 30 },
                ["person2"] = new PersonImplementation { FirstName = "Jane", LastName = "Smith", Age = 25 }
            },
            ProductsById = new Dictionary<string, IProductInterface>
            {
                ["prod1"] = new ProductImplementation { Id = "P1", Name = "Product 1", Price = 99.99m },
                ["prod2"] = new ProductImplementation { Id = "P2", Name = "Product 2", Price = 149.99m }
            }
        };

        // Act
        var result = source.Adapt<DictWithInterfacesDest>();

        // Assert
        result.Should().NotBeNull();
        result.PeopleById.Should().NotBeNull();
        result.PeopleById.Count.Should().Be(2);
        result.PeopleById["person1"].FirstName.Should().Be("John");
        result.PeopleById["person1"].Age.Should().Be(30);
        result.PeopleById["person2"].FirstName.Should().Be("Jane");
        result.PeopleById["person2"].Age.Should().Be(25);

        result.ProductsById.Should().NotBeNull();
        result.ProductsById.Count.Should().Be(2);
        result.ProductsById["prod1"].Id.Should().Be("P1");
        result.ProductsById["prod1"].Price.Should().Be(99.99m);
        result.ProductsById["prod2"].Name.Should().Be("Product 2");
    }

    [Fact]
    public void Adapt_IReadOnlyDictionaryWithInterfaceValues_ShouldMap()
    {
        // Arrange
        var source = new DictWithInterfacesSource
        {
            PeopleById = new Dictionary<string, IPersonInterface>(),
            ProductsById = new Dictionary<string, IProductInterface>
            {
                ["key1"] = new ProductImplementation { Id = "ID1", Name = "Test", Price = 50m }
            }
        };

        // Act
        var result = source.Adapt<DictWithInterfacesDest>();

        // Assert
        result.Should().NotBeNull();
        result.PeopleById.Count.Should().Be(0);
        result.ProductsById.Count.Should().Be(1);
        result.ProductsById["key1"].Name.Should().Be("Test");
        result.ProductsById["key1"].Price.Should().Be(50m);
    }

    [Fact]
    public void Adapt_DictionaryOfInterfaces_ShouldCreateIndependentCopies()
    {
        // Arrange
        var person = new PersonImplementation { FirstName = "Original", LastName = "Name", Age = 30 };
        var source = new DictWithInterfacesSource
        {
            PeopleById = new Dictionary<string, IPersonInterface> { ["test"] = person },
            ProductsById = new Dictionary<string, IProductInterface>()
        };

        // Act
        var result = source.Adapt<DictWithInterfacesDest>();

        // Modify original
        person.FirstName = "Modified";

        // Assert
        result.PeopleById["test"].FirstName.Should().Be("Original"); // Should be unchanged
    }
}

