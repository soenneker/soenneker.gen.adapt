namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Generics;

public class MultiGenericSource<TKey, TValue>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }
}

public class ConcreteKeyValueSource : MultiGenericSource<string, int>
{
    public string Description { get; set; }
}

