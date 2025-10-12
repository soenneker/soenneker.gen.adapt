using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class ReadOnlyListDest
{
    public List<PersonClass> People { get; set; }
    public List<ProductClass> Products { get; set; }
}

