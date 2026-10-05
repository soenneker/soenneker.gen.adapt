using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class DeferredMemberMappingTests
{
    [Test]
    public void Property_of_inferred_adapt_result_generates_same_type_mapping()
    {
        var document = new DeferredDocument();
        new DeferredMetadata().Adapt<DeferredMetadataDocument>();
        var entity = document.Adapt<DeferredEntity>();
        var copy = entity.Metadata.Adapt<DeferredMetadata>();
        copy.Should().NotBeSameAs(entity.Metadata);
        copy.Name.Should().Be("metadata");
        copy.Name = "auction";
        entity.Metadata.Name.Should().Be("metadata");
    }
}
