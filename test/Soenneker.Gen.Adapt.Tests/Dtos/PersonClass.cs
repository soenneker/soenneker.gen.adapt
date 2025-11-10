using Soenneker.Gen.Adapt.Tests.Dtos.Abstract;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class PersonClass : IPersonInterface
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}

