using Soenneker.Gen.Adapt.Tests.Dtos;
using Soenneker.Tests.Unit;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class InheritancePropertyTests : UnitTest
{
    public InheritancePropertyTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_ClassWithDerivedProperties_ShouldMapToNewHierarchy()
    {
        var source = new HierarchySource
        {
            First = new AlphaSource
            {
                Key = "alpha-key",
                Count = 5
            },
            Second = new BetaSource
            {
                Key = "beta-key",
                Description = "Beta description"
            },
            Third = new GammaSource
            {
                Key = "gamma-key",
                Amount = 9.75m
            }
        };

        var result = source.Adapt<HierarchyDest>();

        result.First.Should().NotBeNull();
        result.First.Key.Should().Be("alpha-key");
        result.First.Count.Should().Be(5);

        result.Second.Should().NotBeNull();
        result.Second.Key.Should().Be("beta-key");
        result.Second.Description.Should().Be("Beta description");

        result.Third.Should().NotBeNull();
        result.Third.Key.Should().Be("gamma-key");
        result.Third.Amount.Should().Be(9.75m);
    }
}

