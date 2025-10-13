namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Generics;

public class MultiGenericDest<TKey, TValue>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }
}

public class ConcreteKeyValueDest : MultiGenericDest<string, int>
{
    public string Description { get; set; }
}

