namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance;

public interface IIdentifiableSource
{
    string Id { get; set; }
}

public interface INamedSource
{
    string Name { get; set; }
}

public class EntitySource : IIdentifiableSource, INamedSource
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

