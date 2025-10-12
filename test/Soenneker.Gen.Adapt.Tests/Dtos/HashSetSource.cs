using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class HashSetSource
{
    public HashSet<string> Tags { get; set; }
    public ISet<int> Numbers { get; set; }
    public HashSet<IPersonInterface> People { get; set; }
}

