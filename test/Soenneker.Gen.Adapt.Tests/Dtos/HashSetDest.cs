using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class HashSetDest
{
    public List<string> Tags { get; set; }
    public HashSet<int> Numbers { get; set; }
    public List<PersonClass> People { get; set; }
}

