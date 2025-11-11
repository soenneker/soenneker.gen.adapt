using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Complex;

public class ComplexWithCollectionsDest
{
    public string Name { get; set; }
    public List<string> Tags { get; set; }
    public Dictionary<string, int> Counts { get; set; }
    public List<ChildItemDest> Children { get; set; }
}

