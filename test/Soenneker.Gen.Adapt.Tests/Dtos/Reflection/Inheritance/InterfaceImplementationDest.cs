namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance;

public interface IIdentifiableDest
{
    string Id { get; set; }
}

public interface INamedDest
{
    string Name { get; set; }
}

public class EntityDest : IIdentifiableDest, INamedDest
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

