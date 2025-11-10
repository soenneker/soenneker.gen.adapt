using Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance.Abstract;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance;

public class EntitySource : IIdentifiableSource, INamedSource
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}

