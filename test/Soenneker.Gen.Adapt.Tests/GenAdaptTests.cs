using Soenneker.Tests.Unit;

namespace Soenneker.Gen.Adapt.Tests;

/// <summary>
/// Main test class - most tests have been split into specialized test files.
/// See: BasicMappingTests, CollectionTests, DictionaryTests, IEnumerableTests,
/// NestedCollectionTests, EnumConversionTests, NestedObjectTests, NullableTests,
/// EdgeCaseTests, StructTests, RecordTests, PerformanceTests, ComplexScenarioTests
/// </summary>
public sealed class GenAdaptTests : UnitTest
{
    [Test]
    public void Default()
    {
        // Default test to ensure test project builds
    }
}
