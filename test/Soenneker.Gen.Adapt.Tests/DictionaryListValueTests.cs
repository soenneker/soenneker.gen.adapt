using System.Collections.Generic;
using AwesomeAssertions;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class DictionaryListValueTests : UnitTest
{
    public DictionaryListValueTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_DictionaryWithListValue_ShouldMapListValues()
    {
        var sharedDocument = new ShipmentDocument { Code = "DOC-1" };

        var sourceList = new List<PackageDocument>
        {
            new PackageDocument { Tracking = "PKG-1", Weight = 1.5m },
            new PackageDocument { Tracking = "PKG-2", Weight = 2.25m }
        };

        var source = new ShipmentLedgerSource
        {
            Packages = new Dictionary<ShipmentDocument, List<PackageDocument>>
            {
                [sharedDocument] = sourceList
            }
        };

        var result = source.Adapt<ShipmentLedgerDest>();

        result.Should().NotBeNull();
        result.Packages.Should().NotBeNull();
        result.Packages.Should().ContainKey(sharedDocument);

        List<PackageEntity> mappedList = result.Packages[sharedDocument];
        mappedList.Should().NotBeNull();
        mappedList.Should().HaveCount(2);
        ReferenceEquals(mappedList, sourceList).Should().BeFalse();
        mappedList[0].Tracking.Should().Be("PKG-1");
        mappedList[0].Weight.Should().Be(1.5m);
        mappedList[1].Tracking.Should().Be("PKG-2");
        mappedList[1].Weight.Should().Be(2.25m);

        sourceList[0].Tracking = "UPDATED";
        mappedList[0].Tracking.Should().Be("PKG-1");
    }

    [Fact]
    public void Adapt_DictionaryWithListValue_DirectCall_ShouldMapListValues()
    {
        var sharedDocument = new ShipmentDocument { Code = "DOC-1" };

        var sourceList = new List<PackageDocument>
        {
            new PackageDocument { Tracking = "PKG-1", Weight = 1.5m },
            new PackageDocument { Tracking = "PKG-2", Weight = 2.25m }
        };

        var source = new Dictionary<ShipmentDocument, List<PackageDocument>>
        {
            [sharedDocument] = sourceList
        };

        var result = source.Adapt<Dictionary<ShipmentDocument, List<PackageEntity>>>();

        result.Should().NotBeNull();
        result.Should().ContainKey(sharedDocument);

        List<PackageEntity> mappedList = result[sharedDocument];
        mappedList.Should().NotBeNull();
        mappedList.Should().HaveCount(2);
        ReferenceEquals(mappedList, sourceList).Should().BeFalse();
        mappedList[0].Tracking.Should().Be("PKG-1");
        mappedList[0].Weight.Should().Be(1.5m);
        mappedList[1].Tracking.Should().Be("PKG-2");
        mappedList[1].Weight.Should().Be(2.25m);
    }
}


