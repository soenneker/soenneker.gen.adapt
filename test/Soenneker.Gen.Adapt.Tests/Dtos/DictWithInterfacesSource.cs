using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class DictWithInterfacesSource
{
    public Dictionary<string, IPersonInterface> PeopleById { get; set; }
    public IReadOnlyDictionary<string, IProductInterface> ProductsById { get; set; }
}

