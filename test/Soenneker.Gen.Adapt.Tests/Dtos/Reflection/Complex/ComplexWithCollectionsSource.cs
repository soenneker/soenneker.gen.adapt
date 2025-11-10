using System;
using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Complex;

public class ComplexWithCollectionsSource
{
    public string Name { get; set; }
    public List<string> Tags { get; set; }
    public Dictionary<string, int> Counts { get; set; }
    public List<ChildItemSource> Children { get; set; }
}

