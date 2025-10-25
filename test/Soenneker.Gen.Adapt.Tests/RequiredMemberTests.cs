using Soenneker.Tests.Unit;
using System;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class RequiredMemberTests : UnitTest
{
    public RequiredMemberTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_RequiredMembers_ShouldUseObjectInitializer()
    {
        // Arrange
        var source = new RequiredMemberSource
        {
            Id = "test-123",
            Name = "Test Name",
            Count = 42
        };

        // Act
        var result = source.Adapt<RequiredMemberDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("test-123");
        result.Name.Should().Be("Test Name");
        result.Count.Should().Be(42);
    }

    [Fact]
    public void Adapt_InitOnlyProperties_ShouldUseObjectInitializer()
    {
        // Arrange
        var source = new InitOnlySource
        {
            Id = "init-123",
            Name = "Init Name",
            Count = 100
        };

        // Act
        var result = source.Adapt<InitOnlyDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("init-123");
        result.Name.Should().Be("Init Name");
        result.Count.Should().Be(100);
    }

    [Fact]
    public void Adapt_RegularProperties_ShouldUseObjectInitializer()
    {
        // Arrange
        var source = new RegularSource
        {
            Id = "regular-123",
            Name = "Regular Name",
            Count = 200
        };

        // Act
        var result = source.Adapt<RegularDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("regular-123");
        result.Name.Should().Be("Regular Name");
        result.Count.Should().Be(200);
    }

    [Fact]
    public void Adapt_ShouldAlwaysUseObjectInitializers_ForConsistency()
    {
        // This test documents the expected behavior: all mappings should use object initializers
        // regardless of whether properties are required, init-only, or regular.
        // This ensures consistent behavior and proper handling of all property types.
        
        // Arrange
        var requiredSource = new RequiredMemberSource { Id = "req-123", Name = "Required", Count = 100 };
        var initOnlySource = new InitOnlySource { Id = "init-123", Name = "Init Only", Count = 200 };
        var regularSource = new RegularSource { Id = "reg-123", Name = "Regular", Count = 300 };

        // Act
        var requiredResult = requiredSource.Adapt<RequiredMemberDest>();
        var initOnlyResult = initOnlySource.Adapt<InitOnlyDest>();
        var regularResult = regularSource.Adapt<RegularDest>();

        // Assert - all should work consistently
        requiredResult.Should().NotBeNull();
        requiredResult.Id.Should().Be("req-123");
        requiredResult.Name.Should().Be("Required");
        requiredResult.Count.Should().Be(100);

        initOnlyResult.Should().NotBeNull();
        initOnlyResult.Id.Should().Be("init-123");
        initOnlyResult.Name.Should().Be("Init Only");
        initOnlyResult.Count.Should().Be(200);

        regularResult.Should().NotBeNull();
        regularResult.Id.Should().Be("reg-123");
        regularResult.Name.Should().Be("Regular");
        regularResult.Count.Should().Be(300);
    }

    [Fact]
    public void Adapt_RequiredMembersWithMissingSource_ShouldProvideDefaults()
    {
        // Arrange
        var source = new RequiredMemberSource
        {
            Id = "test-123",
            Name = "Test Name"
            // Count is missing from source
        };

        // Act
        var result = source.Adapt<RequiredMemberDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("test-123");
        result.Name.Should().Be("Test Name");
        result.Count.Should().Be(0); // Default value for int
    }

    [Fact]
    public void Adapt_RequiredMembersWithNullSource_ShouldHandleGracefully()
    {
        // Arrange
        RequiredMemberSource? source = null;

        // Act & Assert
        var action = () => source.Adapt<RequiredMemberDest>();
        action.Should().Throw<NullReferenceException>();
    }
}
