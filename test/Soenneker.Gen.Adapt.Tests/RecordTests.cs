using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class RecordTests : UnitTest
{
    public RecordTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_Record_ShouldMapProperties()
    {
        // Arrange  
        var source = new PersonRecordSource
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30
        };

        // Act
        var result = source.Adapt<PersonRecordDest>();

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Age.Should().Be(30);
    }

    [Fact]
    public void Adapt_Record_EmptyStrings_ShouldMap()
    {
        // Arrange
        var source = new PersonRecordSource
        {
            FirstName = "",
            LastName = "",
            Age = 0
        };

        // Act
        var result = source.Adapt<PersonRecordDest>();

        // Assert
        result.FirstName.Should().Be("");
        result.LastName.Should().Be("");
        result.Age.Should().Be(0);
    }

    [Fact]
    public void Adapt_Record_SpecialCharacters_ShouldMap()
    {
        // Arrange
        var source = new PersonRecordSource
        {
            FirstName = "François",
            LastName = "O'Brien-MacDonald",
            Age = 25
        };

        // Act
        var result = source.Adapt<PersonRecordDest>();

        // Assert
        result.FirstName.Should().Be("François");
        result.LastName.Should().Be("O'Brien-MacDonald");
    }

    [Fact]
    public void Adapt_Record_VeryOldAge_ShouldMap()
    {
        // Arrange
        var source = new PersonRecordSource
        {
            FirstName = "Ancient",
            LastName = "Person",
            Age = 150
        };

        // Act
        var result = source.Adapt<PersonRecordDest>();

        // Assert
        result.Age.Should().Be(150);
    }

    [Fact]
    public void Adapt_Record_MultipleInstances_ShouldBeIndependent()
    {
        // Arrange
        var source1 = new PersonRecordSource { FirstName = "Alice", LastName = "Smith", Age = 25 };
        var source2 = new PersonRecordSource { FirstName = "Bob", LastName = "Jones", Age = 35 };
        var source3 = new PersonRecordSource { FirstName = "Charlie", LastName = "Brown", Age = 45 };

        // Act
        var result1 = source1.Adapt<PersonRecordDest>();
        var result2 = source2.Adapt<PersonRecordDest>();
        var result3 = source3.Adapt<PersonRecordDest>();

        // Assert
        result1.Should().NotBeSameAs(result2);
        result2.Should().NotBeSameAs(result3);
        
        result1.FirstName.Should().Be("Alice");
        result1.Age.Should().Be(25);
        
        result2.FirstName.Should().Be("Bob");
        result2.Age.Should().Be(35);
        
        result3.FirstName.Should().Be("Charlie");
        result3.Age.Should().Be(45);
    }
}

