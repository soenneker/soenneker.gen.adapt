namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Generics;

public class GenericWrapperSource<T>
{
    public T Value { get; set; }
    public string Name { get; set; }
}

public class ConcreteIntWrapperSource : GenericWrapperSource<int>
{
    public string AdditionalInfo { get; set; }
}

public class ConcreteStringWrapperSource : GenericWrapperSource<string>
{
    public bool IsActive { get; set; }
}

