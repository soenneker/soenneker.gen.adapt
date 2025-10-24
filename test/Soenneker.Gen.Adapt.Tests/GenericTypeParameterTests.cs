using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;
using System;

namespace Soenneker.Gen.Adapt.Tests;

/// <summary>
/// Tests for .Adapt() with generic type parameters (TEntity/TDocument scenarios)
/// </summary>
public sealed class GenericTypeParameterTests : UnitTest
{
    public GenericTypeParameterTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_TEntity_To_TDocument_CustomerScenario()
    {
        var entity = new CustomerEntity
        {
            Id = "customer-123",
            CreatedAt = DateTime.UtcNow,
            Name = "John Doe",
            Email = "john@example.com"
        };

        var document = entity.Adapt<CustomerDocument>();

        document.Should().NotBeNull();
        document.Name.Should().Be("John Doe");
        document.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void Adapt_TDocument_To_TEntity_GetScenario()
    {
        var document = new CustomerDocument
        {
            DocumentId = Guid.NewGuid().ToString(),
            PartitionKey = "partition-1",
            Name = "Jane Smith",
            Email = "jane@example.com"
        };

        var entity = document.Adapt<CustomerEntity>();

        entity.Should().NotBeNull();
        entity.Name.Should().Be("Jane Smith");
        entity.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public void Adapt_TEntity_To_TDocument_ProductScenario()
    {
        var entity = new ProductEntity
        {
            Id = "product-456",
            CreatedAt = DateTime.UtcNow,
            ProductName = "Widget",
            Price = 99.99m
        };

        var document = entity.Adapt<ProductDocument>();

        document.Should().NotBeNull();
        document.ProductName.Should().Be("Widget");
        document.Price.Should().Be(99.99m);
    }

    [Fact]
    public void Adapt_RoundTrip_SimulatesYourCreateAndGetMethods()
    {
        var entityToCreate = new CustomerEntity
        {
            Name = "Bob Johnson",
            Email = "bob@example.com",
            CreatedAt = DateTime.UtcNow
        };

        var document = entityToCreate.Adapt<CustomerDocument>();
        document.DocumentId = Guid.NewGuid().ToString();
        document.PartitionKey = document.DocumentId;
        entityToCreate.Id = document.DocumentId;

        var retrievedEntity = document.Adapt<CustomerEntity>();

        retrievedEntity.Name.Should().Be("Bob Johnson");
        retrievedEntity.Email.Should().Be("bob@example.com");
    }

    [Fact]
    public void Adapt_UpdateWorkflow_SimulatesYourUpdateMethod()
    {
        var entity = new CustomerEntity
        {
            Id = "existing-789",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            Name = "Original Name",
            Email = "original@example.com"
        };

        entity.ModifiedAt = DateTime.UtcNow;
        var toUpdateDocument = entity.Adapt<CustomerDocument>();
        toUpdateDocument.DocumentId = entity.Id;
        toUpdateDocument.PartitionKey = entity.Id;

        var result = toUpdateDocument.Adapt<CustomerEntity>();

        result.Name.Should().Be("Original Name");
        result.Email.Should().Be("original@example.com");
    }

    [Fact]
    public void Adapt_MultipleEntityDocumentPairs_AllWork()
    {
        var customerEntity = new CustomerEntity { Name = "Customer1", Email = "c1@test.com" };
        var productEntity = new ProductEntity { ProductName = "Product1", Price = 50m };

        var customerDoc = customerEntity.Adapt<CustomerDocument>();
        var productDoc = productEntity.Adapt<ProductDocument>();

        customerDoc.Name.Should().Be("Customer1");
        productDoc.ProductName.Should().Be("Product1");

        var customerBack = customerDoc.Adapt<CustomerEntity>();
        var productBack = productDoc.Adapt<ProductEntity>();

        customerBack.Email.Should().Be("c1@test.com");
        productBack.Price.Should().Be(50m);
    }
}
