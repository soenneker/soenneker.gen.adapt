using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class EntitiesManagerTests : UnitTest
{
    [Test]
    public void Create_should_create()
    {
        var entitiesManager = new EntitiesManager();

        CustomerEntity resultingEntity = entitiesManager.Create(new CustomerEntity());

        resultingEntity.Should().NotBeNull();
    }
}
