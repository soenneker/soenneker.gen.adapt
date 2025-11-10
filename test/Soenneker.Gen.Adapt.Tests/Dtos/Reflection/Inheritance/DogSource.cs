using Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance.Base;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance;

public class DogSource : AnimalBaseSource
{
    public string Breed { get; set; }
    public bool IsGoodBoy { get; set; }
}

