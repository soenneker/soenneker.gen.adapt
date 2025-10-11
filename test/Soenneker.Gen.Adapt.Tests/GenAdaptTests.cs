using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests;

/// <summary>
/// Main test class - most tests have been split into specialized test files.
/// See: BasicMappingTests, CollectionTests, DictionaryTests, IEnumerableTests,
/// NestedCollectionTests, EnumConversionTests, NestedObjectTests, NullableTests,
/// EdgeCaseTests, StructTests, RecordTests, PerformanceTests, ComplexScenarioTests
/// </summary>
[Collection("Collection")]
public sealed class GenAdaptTests : UnitTest
{
    public GenAdaptTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Default()
    {
        // Default test to ensure test project builds
    }
}
