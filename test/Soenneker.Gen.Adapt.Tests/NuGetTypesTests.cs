using Soenneker.Tests.Unit;
using Xunit;
using Soenneker.Documents.Audit;
using AwesomeAssertions;
using Soenneker.Dtos.IdNamePair;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class NuGetTypesTests : UnitTest
{
    public NuGetTypesTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_Document_should_Adapt()
    {
        var document = new AuditDocument();

        var newDoc = document.Adapt<AuditDocument>();

        newDoc.Should().NotBeNull();
        newDoc.Id.Should().Be(document.Id);
    }

    [Fact]
    public void Adapt_Document_to_IdNamePair_should_adapt()
    {
        var document = AutoFaker.Generate<AuditDocument>();

        var idNamePair = document.Adapt<IdNamePair>();

        idNamePair.Should().NotBeNull();
        idNamePair.Id.Should().Be(document.Id);
    }

    [Fact]
    public void Adapt_IdNamePair_to_Document_should_adapt()
    {
        var idNamePair = AutoFaker.Generate<IdNamePair>();

        var document = idNamePair.Adapt<AuditDocument>();

        document.Should().NotBeNull();
        document.Id.Should().Be(document.Id);
    }
}

