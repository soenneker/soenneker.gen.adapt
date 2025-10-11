using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

[Collection("Collection")]
public sealed class EnumConversionTests : UnitTest
{
    public EnumConversionTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_EnumToString_ShouldConvert()
    {
        // Arrange
        var source = new EnumSource
        {
            Status = TestStatus.Active,
            Priority = 1
        };

        // Act
        var result = source.Adapt<EnumToStringDest>();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Active");
    }

    [Fact]
    public void Adapt_StringToEnum_ShouldParse()
    {
        // Arrange
        var source = new StringToEnumSource
        {
            StatusString = "Pending"
        };

        // Act
        var result = source.Adapt<StringToEnumDest>();

        // Assert
        result.Should().NotBeNull();
        result.StatusString.Should().Be(TestStatus.Pending);
    }

    [Fact]
    public void Adapt_EnumToInt_ShouldCast()
    {
        // Arrange
        var source = new EnumToIntSource
        {
            Status = TestStatus.Completed
        };

        // Act
        var result = source.Adapt<EnumToIntDest>();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(2);
    }

    [Fact]
    public void Adapt_IntToEnum_ShouldCast()
    {
        // Arrange
        var source = new IntToEnumSource
        {
            StatusCode = 1
        };

        // Act
        var result = source.Adapt<IntToEnumDest>();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(TestStatus.Pending);
    }

    [Fact]
    public void Adapt_IntellenumToInt_ShouldExtractValue()
    {
        // Arrange
        var source = new IntellenumSource
        {
            UserId = TestIntellenum.From(999)
        };

        // Act
        var result = source.Adapt<IntellenumToIntDest>();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(999);
    }

    [Fact]
    public void Adapt_IntToIntellenum_ShouldCreateFrom()
    {
        // Arrange
        var source = new IntToIntellenumSource
        {
            UserId = 777
        };

        // Act
        var result = source.Adapt<IntToIntellenumDest>();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().NotBeNull();
        result.UserId.Value.Should().Be(777);
    }

    [Fact]
    public void Adapt_EnumToString_AllValues_ShouldConvert()
    {
        // Test Active
        var sourceActive = new EnumSource { Status = TestStatus.Active, Priority = 1 };
        var resultActive = sourceActive.Adapt<EnumToStringDest>();
        resultActive.Status.Should().Be("Active");

        // Test Pending
        var sourcePending = new EnumSource { Status = TestStatus.Pending, Priority = 1 };
        var resultPending = sourcePending.Adapt<EnumToStringDest>();
        resultPending.Status.Should().Be("Pending");

        // Test Completed
        var sourceCompleted = new EnumSource { Status = TestStatus.Completed, Priority = 1 };
        var resultCompleted = sourceCompleted.Adapt<EnumToStringDest>();
        resultCompleted.Status.Should().Be("Completed");
    }

    [Fact]
    public void Adapt_IntToEnum_AllValues_ShouldCast()
    {
        // Test 0 -> Active
        var source0 = new IntToEnumSource { StatusCode = 0 };
        source0.Adapt<IntToEnumDest>().StatusCode.Should().Be(TestStatus.Active);

        // Test 1 -> Pending
        var source1 = new IntToEnumSource { StatusCode = 1 };
        source1.Adapt<IntToEnumDest>().StatusCode.Should().Be(TestStatus.Pending);

        // Test 2 -> Completed
        var source2 = new IntToEnumSource { StatusCode = 2 };
        source2.Adapt<IntToEnumDest>().StatusCode.Should().Be(TestStatus.Completed);
    }

    [Fact]
    public void Adapt_EnumToInt_AllValues_ShouldCast()
    {
        // Test Active -> 0
        var sourceActive = new EnumToIntSource { Status = TestStatus.Active };
        sourceActive.Adapt<EnumToIntDest>().Status.Should().Be(0);

        // Test Pending -> 1
        var sourcePending = new EnumToIntSource { Status = TestStatus.Pending };
        sourcePending.Adapt<EnumToIntDest>().Status.Should().Be(1);

        // Test Completed -> 2
        var sourceCompleted = new EnumToIntSource { Status = TestStatus.Completed };
        sourceCompleted.Adapt<EnumToIntDest>().Status.Should().Be(2);
    }

    [Fact]
    public void Adapt_IntellenumToInt_ZeroValue_ShouldExtract()
    {
        // Arrange
        var source = new IntellenumSource { UserId = TestIntellenum.From(0) };

        // Act
        var result = source.Adapt<IntellenumToIntDest>();

        // Assert
        result.UserId.Should().Be(0);
    }

    [Fact]
    public void Adapt_IntellenumToInt_NegativeValue_ShouldExtract()
    {
        // Arrange
        var source = new IntellenumSource { UserId = TestIntellenum.From(-42) };

        // Act
        var result = source.Adapt<IntellenumToIntDest>();

        // Assert
        result.UserId.Should().Be(-42);
    }

    [Fact]
    public void Adapt_IntellenumToInt_MaxValue_ShouldExtract()
    {
        // Arrange
        var source = new IntellenumSource { UserId = TestIntellenum.From(int.MaxValue) };

        // Act
        var result = source.Adapt<IntellenumToIntDest>();

        // Assert
        result.UserId.Should().Be(int.MaxValue);
    }
}

