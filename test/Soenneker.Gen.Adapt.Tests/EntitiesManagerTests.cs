using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class EntitiesManagerTests : UnitTest
{
    public EntitiesManagerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Create_should_create()
    {
        var entitiesManager = new EntitiesManager<CustomerEntity, CustomerDocument>();

        CustomerEntity resultingEntity = entitiesManager.Create(new CustomerEntity());

        resultingEntity.Should().NotBeNull();
    }
}
