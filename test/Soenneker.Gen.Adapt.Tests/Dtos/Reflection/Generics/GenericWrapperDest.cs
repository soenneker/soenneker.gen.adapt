namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Generics;

public class GenericWrapperDest<T>
{
    public T Value { get; set; }
    public string Name { get; set; }
}

public class ConcreteIntWrapperDest : GenericWrapperDest<int>
{
    public string AdditionalInfo { get; set; }
}

public class ConcreteStringWrapperDest : GenericWrapperDest<string>
{
    public bool IsActive { get; set; }
}

