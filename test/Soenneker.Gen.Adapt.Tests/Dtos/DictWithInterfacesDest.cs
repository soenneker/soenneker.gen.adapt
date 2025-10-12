using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class DictWithInterfacesDest
{
    public Dictionary<string, PersonClass> PeopleById { get; set; }
    public Dictionary<string, ProductClass> ProductsById { get; set; }
}

