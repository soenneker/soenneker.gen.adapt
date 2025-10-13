using System;
using AwesomeAssertions;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests.Reflection;

public sealed class ReflectionAdapterTests : UnitTest
{
    public ReflectionAdapterTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void AdaptViaReflection_should_copy_all_properties()
    {
        // Arrange
        var source = new BasicSource
        {
            Id = "test-id",
            Name = "Test Name",
            Count = 42
        };

        // Act
        var result = source.AdaptViaReflection<BasicDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
        result.Name.Should().Be(source.Name);
        result.Count.Should().Be(source.Count);
    }

    [Fact]
    public void AdaptViaReflection_should_work_with_derived_types()
    {
        // Arrange
        var source = new CustomerEntity
        {
            Name = "John Doe",
            Email = "john@example.com"
        };

        // Act
        var result = source.AdaptViaReflection<CustomerDocument>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(source.Name);
        result.Email.Should().Be(source.Email);
    }

    [Fact]
    public void AdaptViaReflection_should_handle_string_properties()
    {
        // Arrange
        var source = new NestedSource
        {
            Name = "Parent Name",
            Child = new BasicSource { Id = "test", Name = "child", Count = 1 }
        };

        // Act
        var result = source.AdaptViaReflection<NestedDest>();

        // Assert - Only Name is copied (string is assignable), Child is not (BasicSource -> BasicDest requires conversion)
        result.Should().NotBeNull();
        result.Name.Should().Be(source.Name);
        // Note: Child won't be copied since BasicSource is not assignable to BasicDest
        // For nested object mapping, use the regular Adapt<>() method
    }

    [Fact]
    public void AdaptViaReflection_should_cache_mappers()
    {
        // Arrange
        var source1 = new BasicSource { Id = "1", Name = "First", Count = 10 };
        var source2 = new BasicSource { Id = "2", Name = "Second", Count = 20 };

        // Act - Call twice with same types to test caching
        var result1 = source1.AdaptViaReflection<BasicDest>();
        var result2 = source2.AdaptViaReflection<BasicDest>();

        // Assert
        result1.Id.Should().Be("1");
        result1.Name.Should().Be("First");
        result1.Count.Should().Be(10);
        result2.Id.Should().Be("2");
        result2.Name.Should().Be("Second");
        result2.Count.Should().Be(20);
    }

    [Fact]
    public void AdaptViaReflection_should_only_copy_matching_properties()
    {
        // Arrange
        var source = new PartialMatchSource
        {
            Id = "partial-id",
            Name = "Partial Name",
            ExtraField = "This should not be copied"
        };

        // Act
        var result = source.AdaptViaReflection<PartialMatchDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
        result.Name.Should().Be(source.Name);
        // ExtraField doesn't exist on PartialMatchDest, so it's not copied
    }

    [Fact]
    public void AdaptViaReflection_should_throw_on_null_source()
    {
        // Arrange
        BasicSource? source = null;

        // Act & Assert
        Action act = () => source!.AdaptViaReflection<BasicDest>();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AdaptViaReflection_reverse_mapping_should_work()
    {
        // Arrange
        var source = new CustomerDocument
        {
            Name = "Jane Doe",
            Email = "jane@example.com"
        };

        // Act - Reverse mapping: Document -> Entity
        var result = source.AdaptViaReflection<CustomerEntity>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(source.Name);
        result.Email.Should().Be(source.Email);
    }
}

